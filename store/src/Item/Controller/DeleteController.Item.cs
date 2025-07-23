using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("item")]
public class DeleteItemController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    private readonly Neo4jQuery neo4JQuery;
    private const string ITEM = "Item";
    private const string RETURN = "RETURN";
    public DeleteItemController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        this.driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        neo4JQuery = new();
    }
    [Route("delete/{id}")]
    [HttpDelete]

    public async Task<ActionResult> ObrisiItemAsync(int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var testQuety = neo4JQuery.QueryByOneElement(ITEM,"id","id",RETURN);
            var deleteQuery = @"
            MATCH (n:Item {id: $id})
            DETACH DELETE n";
            var parameters = new Dictionary<string, object>
            {
                { "id", id }
            };

            var result = await neo4JQuery.ExecuteReadAsync(session,testQuety,parameters);
            if (result != null)
            {
                var res = await neo4JQuery.ExecuteWriteAsync(session,deleteQuery,parameters);
                return Ok("Node deleted successfully");
            }
            else return NotFound(new { message = "Not found" });

        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}