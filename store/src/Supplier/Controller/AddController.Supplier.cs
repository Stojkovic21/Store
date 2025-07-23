using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("supplier")]
public class AddSupplierController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    private readonly Neo4jQuery neo4JQuery;
    private const string SUPPLIER = "Supplier";
    private const string RETURN = "RETURN";
    public AddSupplierController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        neo4JQuery = new();
    }
    [Route("add")]
    [HttpPost]
    public async Task<ActionResult> AddSupplierAsync([FromBody] SupplierModel supplierModel)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();
            var testQuety = neo4JQuery.QueryByOneElement(SUPPLIER,"id","id",RETURN);
            var query = @"
            CREATE(n:Supplier {id:$id, name:$name, email: $email})";
            var parameters = new Dictionary<string, object>
            {
                {"id",supplierModel.Id},
                {"name",supplierModel.Name},
                {"email",supplierModel.Email},
            };
            var result = await neo4JQuery.ExecuteReadAsync(session,testQuety,parameters);
            if (result == null)
            {
                var res = await neo4JQuery.ExecuteWriteAsync(session,query,parameters);
                return Ok("Nodes added successfully!");
            }
            else return NotFound(new { message = "Node existing" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}