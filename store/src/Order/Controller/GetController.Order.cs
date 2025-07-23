using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("order")]
public class GetOrderController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    private readonly Neo4jQuery neo4JQuery;
    private const string ORDER = "Order";
    private const string RETURN = "RETURN";
    public GetOrderController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        neo4JQuery = new();
    }
    [HttpGet]
    [Route("get/all")]
    public async Task<ActionResult> GetAllCategorysAsync()
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = @"
            MATCH (n:Order)
            RETURN n";
            var parameters = new Dictionary<string, object>
            {
            };
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var response = await tx.RunAsync(query, parameters);
                var orders = new List<INode>();
                while (await response.FetchAsync())
                {
                    orders.Add(response.Current["n"].As<INode>());
                }
                return orders;
            });
            if (result != null)
            {
                return Ok(new
                {
                    message = "True",
                    orders = result.Select(s => s.Properties)
                });
            }
            else return NotFound(new { message = "Order is not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
    [HttpGet]
    [Route("get/{id}")]
    public async Task<ActionResult> GetOrderAsync(int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = neo4JQuery.QueryByOneElement(ORDER,"id","id",RETURN);
            var parameters = new Dictionary<string, object>
            {
                {"id",id}
            };
            var result = await neo4JQuery.ExecuteReadAsync(session,query,parameters);
            if (result != null)
            {
                return Ok(new
                {
                    message = "True",
                    Category = result.Properties
                });
            }
            else return NotFound(new { message = "Order is not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
}