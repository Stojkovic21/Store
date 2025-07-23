using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("customer")]
public class DeleteCustomerController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    private readonly Neo4jQuery neo4JQuery;
    private const string DELETE="DELETE";
    private const string CUSTOMER = "Customer";
    private const string RETURN = "RETURN";
    public DeleteCustomerController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        neo4JQuery = new();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    [Route("auth")]
    public IActionResult AutenticateOnlyEndpoint()
    {
        return Ok("You are authenticated");
    }

    //[Authorize(Roles = "Admin,User")]
    [HttpDelete]
    [Route("delete/{id}")]
    public async Task<ActionResult> DeleteCustomerAsync(int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var parameters = new Dictionary<string, object>
            {
                {"id",id}
            };
            var testQuety = neo4JQuery.QueryByOneElement(CUSTOMER,"id","id",RETURN);
            var deleteQuery = neo4JQuery.QueryByOneElement(CUSTOMER, "id", "id", DELETE);
            var result = await neo4JQuery.ExecuteReadAsync(session,testQuety,parameters);
            if (result != null)
            {
                await session.ExecuteWriteAsync(async tx =>
                {
                    await tx.RunAsync(deleteQuery, parameters);
                    return Ok(new { message = "Nodes deleted successfully!" });
                });
                return BadRequest("false");
            }
            else return NotFound(new { message = "Not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
}