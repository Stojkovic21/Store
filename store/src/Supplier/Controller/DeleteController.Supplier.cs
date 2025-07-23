using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("supplier")]
public class DeleteSupplierController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    private readonly Neo4jQuery neo4JQuery;
    private const string SUPPLIER = "Supplier";
    private const string RETURN = "RETURN";
    public DeleteSupplierController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        neo4JQuery = new();
    }
    [HttpDelete]
    [Route("delete/{id}")]
    public async Task<ActionResult> DeleteSupplierAsync(int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var testQuety = neo4JQuery.QueryByOneElement(SUPPLIER,"id","id",RETURN);
            var deleteQuery = @"
            MATCH (n:Supplier {id: $id})
            DETACH DELETE n";
            var parameters = new Dictionary<string, object>
            {
                {"id",id}
            };
            var result = await neo4JQuery.ExecuteReadAsync(session,testQuety,parameters);
            if (result != null)
            {
                var res = await neo4JQuery.ExecuteWriteAsync(session,deleteQuery,parameters);
                return Ok("Nodes deleted successfully!");
            }
            else return NotFound(new { message = "Not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
}