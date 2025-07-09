using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

public class Relationsip : ControllerBase
{
    private IDriver driver;
    public Relationsip(IDriver driver)
    {
        this.driver = driver;
    }
    public async Task<ActionResult> Relationship(string query, Dictionary<string, object> parameters)
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
                    message = "Successfully!",
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