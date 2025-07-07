using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("supplier")]
public class EditSupplierController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public EditSupplierController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [HttpPut]
    [Route("put/{id}")]
    public async Task<ActionResult> EditSupplierAsync([FromBody] SupplierModel updateSupplier, int id)
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
                        {"ime",updateSupplier.Ime},
                        {"email",updateSupplier.Email},
                    }
                }
            };
            var query = @"
            MATCH (n:Supplier {id: $id})
            SET n+=$propertis
            RETURN n";

            var updatedNode = await session.ExecuteWriteAsync(async tx =>
            {
                var response = await tx.RunAsync(query, parameters);
                if (await response.FetchAsync())
                {
                    return response.Current["n"].As<INode>();
                }
                return null;
            });
            if (updatedNode != null)
            {
                return Ok(new
                {
                    message = "Supplier updated seccessfully",
                    updatedCategoty = updatedNode.Properties
                });
            }
            else return NotFound(new { mesage = "Supplier not found" });
        }
        catch (System.Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
}
