using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("item")]
public class DeleteItemController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public DeleteItemController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        this.driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [Route("delete/{id}")]
    [HttpDelete]

    public async Task<ActionResult> ObrisiItemAsync(int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var testQuety = @"
            MATCH (n:Item {id: $id})
            RETURN n";
            var deleteQuery = @"
            MATCH (n:Item {id: $id})
            DETACH DELETE n";
            var parameters = new Dictionary<string, object>
            {
                { "id", id }
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
            if (result != null)
            {
                var res = await session.ExecuteWriteAsync(async tx =>
                {
                    await tx.RunAsync(deleteQuery, parameters);
                    return new { message = "Nodes deleted successfully!" };
                });
                return Ok(res);
            }
            else return NotFound(new { message = "Not found" });

        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}