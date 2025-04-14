using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public OrderController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [Route("Dodaj")]
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
                var res=await session.ExecuteWriteAsync(async tx =>
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
    [HttpGet]
    [Route("Get/{id}")]
    public async Task<ActionResult> GetOrderAsync(int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = @"
            MATCH (n:Order {id: $id})
            RETURN n";
            var parameters = new Dictionary<string, object>
            {
                {"id",id}
            };
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var response = await tx.RunAsync(query, parameters);
                if (await response.FetchAsync())
                {
                    return response.Current["n"].As<INode>();
                }
                return null;
            });
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
    [HttpDelete]
    [Route("Delete/{id}")]
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
                var res= await session.ExecuteWriteAsync(async tx =>
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
    [HttpPut]
    [Route("Put/{id}")]
    public async Task<ActionResult> EditOrderAsync([FromBody] OrderModel updateOrder,int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session=driver.AsyncSession();

            var parameters = new Dictionary<string,object>
            {
                {"id", id},
                {
                    "propertis",new Dictionary<string,object>{
                        {"date",updateOrder.Date},
                        {"ukupnaCena",updateOrder.UkupnaCena},
                    }
                }
            };
            var query=@"
            MATCH (n:Order {id: $id})
            SET n+=$propertis
            RETURN n";

            var updatedNode=await session.ExecuteWriteAsync(async tx=>
            {
                var response=await tx.RunAsync(query,parameters);
                if (await response.FetchAsync())
                {
                    return response.Current["n"].As<INode>();
                }
                return null;
            });
            if(updatedNode!=null)
            {
                return Ok (new
                {
                    message="Order updated seccessfully",
                    updatedCategoty=updatedNode.Properties
                });
            }
            else return NotFound(new{mesage="Order not found"});
        }
        catch (System.Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
    [HttpPost]
    [Route("Relationship/Customer/{sourceId}/{targetId}/{relationshipType}")]
    public async Task<ActionResult> CreateRelationshiCustomerpAsync(int sourceId, int targetId, string relationshipType)
    {
        var query = @"
            MATCH (a:Order {id: $sourceId})
            MATCH (b:Customer {id: $targetId})
            MERGE (a)-[r:" + relationshipType + @"]->(b)
            RETURN a, r, b";

        // Create parameters for the query
        var parameters = new Dictionary<string, object>
        {
            { "sourceId", sourceId },
            { "targetId", targetId }
        };

        return await Relationship(query,parameters);
    }
    [HttpDelete]
    [Route("Relationship/Customer/{sourceId}/{targetId}/{relationshipType}")]
    public async Task<ActionResult> DeleteRelationshipCustomerAsync(int sourceId, int targetId, string relationshipType)
    {
        var query = @"
            MATCH (a:Order {id: $sourceId})-[r:" + relationshipType + @"]->(b:Customer {id: $targetId})
            DELETE r
            RETURN a, b";

        // Create parameters for the query
        var parameters = new Dictionary<string, object>
        {
            { "sourceId", sourceId },
            { "targetId", targetId }
        };

        return await Relationship(query,parameters);
    }
    [HttpPost]
    [Route("Relationship/Item/{sourceId}/{targetId}/{relationshipType}")]
    public async Task<ActionResult> CreateRelationshiItemAsync(int sourceId, int targetId, string relationshipType)
    {
        var query = @"
            MATCH (a:Order {id: $sourceId})
            MATCH (b:Item {id: $targetId})
            MERGE (a)-[r:" + relationshipType + @"]->(b)
            RETURN a, r, b";

        // Create parameters for the query
        var parameters = new Dictionary<string, object>
        {
            { "sourceId", sourceId },
            { "targetId", targetId }
        };

        return await Relationship(query,parameters);
    }
    [HttpDelete]
    [Route("Relationship/Item/{sourceId}/{targetId}/{relationshipType}")]
    public async Task<ActionResult> DeleteRelationshipItemAsync(int sourceId, int targetId, string relationshipType)
    {
        var query = @"
            MATCH (a:Order {id: $sourceId})-[r:" + relationshipType + @"]->(b:Item {id: $targetId})
            DELETE r
            RETURN a, b";

        // Create parameters for the query
        var parameters = new Dictionary<string, object>
        {
            { "sourceId", sourceId },
            { "targetId", targetId }
        };

        return await Relationship(query,parameters);
    }
    private async Task<ActionResult> Relationship(string query, Dictionary<string, object> parameters)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session=driver.AsyncSession();
            // Execute the query in a write transaction
            var result = await session.ExecuteWriteAsync(async tx =>
            {
                var response = await tx.RunAsync(query, parameters);

                // Check if the query returned any result
                if (await response.FetchAsync())
                {
                    return new
                    {
                        Source = response.Current["a"].As<INode>().Properties,
                        Target = response.Current["b"].As<INode>().Properties
                    };
                }
                return null;
            });

            if (result != null)
            {
                return Ok(new
                {
                    message = "Relationship deleted successfully!",
                    nodes = result
                });
            }
            else
            {
                return NotFound(new { message = "Relationship or nodes not found!" });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
