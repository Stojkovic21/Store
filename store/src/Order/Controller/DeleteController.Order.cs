using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("order")]
public class DeleteOrderController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public DeleteOrderController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }


    [HttpDelete]
    [Route("delete/{id}")]
    public async Task<ActionResult> DeleteOrderAsync(int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var parameters = new Dictionary<string, object>
            {
                {"id",id}
            };
            var testQuety = @"
            MATCH (n:Order {id: $id})
            RETURN n";
            var deleteQuery = @"
            MATCH (n:Order {id: $id})
            DELETE n";
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
                    return "Nodes deleted successfully!";
                });
                return Ok(res);
            }
            else return NotFound(new { message = "Not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
}