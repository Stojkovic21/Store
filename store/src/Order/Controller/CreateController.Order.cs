using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("order")]
public class CreateOrderController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public CreateOrderController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [Route("create")]
    [HttpPost]
    public async Task<ActionResult> AddOrderAsync([FromBody] OrderModel orderModel)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();
            var testQuety = @"
            MATCH (n:Order {id: $id})
            RETURN n";
            var query = @"
            CREATE(n:Order {id:$id, date:$date, ukupnaCena: $ukupnaCena})";
            var parameters = new Dictionary<string, object>
            {
                {"id",orderModel.Id},
                {"date",orderModel.Date},
                {"ukupnaCena",orderModel.UkupnaCena},
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