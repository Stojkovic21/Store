using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("order")]
public class EditOrderController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    private readonly Neo4jQuery neo4JQuery;
    private const string ORDER = "Order";
    private const string RETURN = "RETURN";
    public EditOrderController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        neo4JQuery = new();
    }
    [HttpPut]
    [Route("put/{id}")]
    public async Task<ActionResult> EditOrderAsync([FromBody] OrderModel updateOrder, int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var parameters = new Dictionary<string, object>
            {
                {"id", id},
                {
                    "propertis",new Dictionary<string,object>{
                        {"date",updateOrder.Date},
                        {"ukupnaCena",updateOrder.UkupnaCena},
                    }
                }
            };
            var query = @"
            MATCH (n:Order {id: $id})
            SET n+=$propertis
            RETURN n";

            var updatedNode = await neo4JQuery.ExecuteWriteAsync(session,query,parameters);
            if (updatedNode != null)
            {
                return Ok(new
                {
                    message = "Order updated seccessfully",
                    updatedCategoty = updatedNode.Properties
                });
            }
            else return NotFound(new { mesage = "Order not found" });
        }
        catch (System.Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
}
