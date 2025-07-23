using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("order")]
public class CreateOrderController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    private readonly Neo4jQuery neo4JQuery;
    private const string ORDER = "Order";
    private const string RETURN = "RETURN";
    public CreateOrderController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        neo4JQuery = new();
    }
    [Route("create")]
    [HttpPost]
    public async Task<ActionResult> AddOrderAsync([FromBody] OrderModel orderModel)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();
            var testQuety = neo4JQuery.QueryByOneElement(ORDER,"id","id",RETURN);
            var query = @"
            CREATE(n:Order {id:$id, date:$date, ukupnaCena: $ukupnaCena})";
            var parameters = new Dictionary<string, object>
            {
                {"id",orderModel.Id},
                {"date",orderModel.Date},
                {"ukupnaCena",orderModel.UkupnaCena},
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