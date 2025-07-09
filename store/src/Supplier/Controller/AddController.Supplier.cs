using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("supplier")]
public class AddSupplierController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public AddSupplierController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [Route("add")]
    [HttpPost]
    public async Task<ActionResult> AddSupplierAsync([FromBody] SupplierModel supplierModel)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();
            var testQuety = @"
            MATCH (n:Supplier {id: $id})
            RETURN n";
            var query = @"
            CREATE(n:Supplier {id:$id, name:$name, email: $email})";
            var parameters = new Dictionary<string, object>
            {
                {"id",supplierModel.Id},
                {"name",supplierModel.Name},
                {"email",supplierModel.Email},
            };
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var response = await tx.RunAsync(testQuety, parameters);
                if (await response.FetchAsync())
                {
                    return response.Current["n"].As<INode>();
                }
                return null;
            });
            if (result == null)
            {
                var res = await session.ExecuteWriteAsync(async tx =>
                {
                    await tx.RunAsync(query, parameters);
                    return "Nodes added successfully!";
                });
                return Ok(res);
            }
            else return NotFound(new { message = "Node existing" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}