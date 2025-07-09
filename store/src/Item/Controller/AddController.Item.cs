using Item.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
[ApiController]
[Route("item")]
public class AddItemController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public AddItemController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        this.driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [Route("add")]
    [HttpPost]
    public async Task<ActionResult> AddItemAsync([FromBody] ItemModel item)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();
            //if (item.Name == "" || item.Brend) return BadRequest("Unesite Naziv i Brand");
            var testQuety = @"
            MATCH (n:Item {id: $id})
            RETURN n";
            var query = @"
            CREATE (n:Item {id:$id, name: $name, price: $price, netoQuantity: $netoQuantity, availableQuantity: $availableQuantity})";

            var parameters = new Dictionary<string, object>
            {
                { "id", item.Id },
                { "name", item.Name },
                { "price", item.Price },
                { "netoQuantity", item.NetoQuantity },
                { "availableQuantity", item.AvailableQuantity },
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
            else return NotFound(new { message = "Node " + item.Id + " existing" });
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex);
            return BadRequest(ex);
        }
    }
}