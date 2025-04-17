using Item.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Neo4j.Driver;
//using Models;
namespace Store.controller;

[ApiController]
[Route("[controller]")]
public class ItemController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;

    public ItemController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [Route("Dodaj")]
    [HttpPost]
    public async Task<ActionResult> AddItemAsync([FromBody] ItemModel item)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();
            if (item.Naziv == "" || item.Brend == "") return BadRequest("Unesite Naziv i Brand");
            var testQuety = @"
            MATCH (n:Item {id: $id})
            RETURN n";
            var query = @"
            CREATE (n:Item {id:$id, naziv: $naziv, cena: $cena, grama: $grama, dostupnaKolicina: $dostupnaKolicina, brend: $brend})";

            var parameters = new Dictionary<string, object>
            {
                { "id", item.Id },
                { "naziv", item.Naziv },
                { "cena", item.Cena },
                { "grama", item.Grama },
                { "dostupnaKolicina", item.DostupnaKolicina },
                { "brend", item.Brend }
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
            //Console.WriteLine(ex);
            return BadRequest(ex);
        }
    }

    [Route("Obrisi/{id}")]
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
            return BadRequest(ex);
        }
    }
    [Route("Get/all")]
    [HttpGet]
    public async Task<ActionResult> GetAllItemsAsync()
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = @"MATCH (n:Item) RETURN n";
            var parameters = new Dictionary<string, object> { };

            var result = await session.ExecuteReadAsync(async tx =>
            {
                var response = await tx.RunAsync(query, parameters);
                var nodes = new List<INode>();
                while (await response.FetchAsync())
                {
                    nodes.Add(response.Current["n"].As<INode>());
                }

                return nodes;
            });

            if (result != null)
            {
                var items = result.Select(node => node.Properties).ToList();

                return Ok(new
                {
                    message = "True",
                    items
                });
            }
            else
                return NotFound(new { message = "False" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
    [Route("Get/id:{id}")]
    [HttpGet]
    public async Task<ActionResult> GetItemByIdAsync(int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = @"
            MATCH (n:Item {id:$id})
            RETURN n";

            // Define default values for missing fields
            var parameters = new Dictionary<string, object>
            {
                { "id", id }
            };

            // Execute the query in a write transaction
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
                var properties = result.Properties;

                return Ok(new
                {
                    message = "True",
                    item = properties
                });
            }
            else
                return NotFound(new { message = "Item not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }

    [Route("Get/naziv:{naziv}")]
    [HttpGet]
    public async Task<ActionResult> GetItemNameAsync(string naziv)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = @"
            MATCH (n:Item {naziv:$naziv})
            RETURN n";

            // Define default values for missing fields
            var parameters = new Dictionary<string, object>
            {
                { "naziv", naziv }
            };

            // Execute the query in a write transaction
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
                var properties = result.Properties;

                return Ok(new
                {
                    message = "True",
                    item = properties
                });
            }
            else
                return NotFound(new { message = "Item not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
    [HttpPut]
    [Route("Put/{id}")]
    public async Task<ActionResult> EditItemAsync([FromBody] ItemModel updatedItem, int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();
            var query = @"
            MATCH (n:Item {id: $id})
            SET n += $properties
            RETURN n";

            // Create parameters for the query
            var parameters = new Dictionary<string, object>
            {
                { "id", id },
                { "properties", new Dictionary<string, object>
                    {
                        { "naziv", updatedItem.Naziv },
                        { "cena", updatedItem.Cena },
                        { "grama",updatedItem.Grama},
                        { "dostupnaKolicina", updatedItem.DostupnaKolicina },
                        { "brend", updatedItem.Brend }
                    }
                }
            };
            var updatedNode = await session.ExecuteWriteAsync(async tx =>
            {
                var response = await tx.RunAsync(query, parameters);

                // Check if the query returned a node
                if (await response.FetchAsync())
                {
                    return response.Current["n"].As<INode>();
                }

                return null;
            });

            // If the node was updated successfully
            if (updatedNode != null)
            {
                var properties = updatedNode.Properties;

                return Ok(new
                {
                    message = "Item updated successfully!",
                    updatedItem = properties
                });
            }
            else
            {
                return NotFound(new { message = "Item not found!" });
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex);
            return BadRequest(ex);
        }
    }
    [HttpPost]
    [Route("Relationship/Category/{sourceId}/{targetId}/{relationshipType}")]
    public async Task<ActionResult> CreateRelationshipCategoryAsync(int sourceId, int targetId, string relationshipType)
    {
        var query = @"
            MATCH (a:Item {id: $sourceId})
            MATCH (b:Category {id: $targetId})
            MERGE (a)-[r:" + relationshipType + @"]->(b)
            RETURN a, r, b";

        // Create parameters for the query
        var parameters = new Dictionary<string, object>
        {
            { "sourceId", sourceId },
            { "targetId", targetId }
        };

        return await Relationship(query, parameters);
    }
    [HttpDelete]
    [Route("Relationship/Category/{sourceId}/{targetId}/{relationshipType}")]
    public async Task<ActionResult> DeleteRelationshipCategoryAsync(int sourceId, int targetId, string relationshipType)
    {
        var query = @"
            MATCH (a:Item {id: $sourceId})-[r:" + relationshipType + @"]->(b:Category {id: $targetId})
            DELETE r
            RETURN a, b";

        // Create parameters for the query
        var parameters = new Dictionary<string, object>
        {
            { "sourceId", sourceId },
            { "targetId", targetId }
        };

        return await Relationship(query, parameters);
    }
    [HttpPost]
    [Route("Relationship/Supplier/{sourceId}/{targetId}/{relationshipType}")]
    public async Task<ActionResult> CreateRelationshipSupplierAsync(int sourceId, int targetId, string relationshipType)
    {
        var query = @"
            MATCH (a:Item {id: $sourceId})
            MATCH (b:Supplier {id: $targetId})
            MERGE (a)-[r:" + relationshipType + @"]->(b)
            RETURN a, r, b";

        // Create parameters for the query
        var parameters = new Dictionary<string, object>
        {
            { "sourceId", sourceId },
            { "targetId", targetId }
        };

        return await Relationship(query, parameters);
    }
    [HttpDelete]
    [Route("Relationship/Supplier/{sourceId}/{targetId}/{relationshipType}")]
    public async Task<ActionResult> DeleteRelationshipSupplierAsync(int sourceId, int targetId, string relationshipType)
    {
        var query = @"
            MATCH (a:Item {id: $sourceId})-[r:" + relationshipType + @"]->(b:Supplier {id: $targetId})
            DELETE r
            RETURN a, b";

        // Create parameters for the query
        var parameters = new Dictionary<string, object>
        {
            { "sourceId", sourceId },
            { "targetId", targetId }
        };

        return await Relationship(query, parameters);
    }
    private async Task<ActionResult> Relationship(string query, Dictionary<string, object> parameters)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();
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
