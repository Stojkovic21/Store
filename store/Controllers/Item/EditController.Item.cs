using Item.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("item")]
public class EditItemController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public EditItemController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        this.driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [HttpPut]
    [Route("put/{id}")]
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
}