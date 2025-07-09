using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("supplier")]
public class GetSupplierController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public GetSupplierController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [HttpGet]
    [Route("get/all")]
    public async Task<ActionResult> GetAllSuppliersAsync()
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = @"
            MATCH (n:Supplier)
            RETURN n";
            var parameters = new Dictionary<string, object>
            {
            };
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var response = await tx.RunAsync(query, parameters);
                var suppliers = new List<INode>();
                while (await response.FetchAsync())
                {
                    suppliers.Add(response.Current["n"].As<INode>());
                }
                return suppliers;
            });
            if (result != null)
            {
                return Ok(new
                {
                    message = "True",
                    suppliers = result.Select(s => s.Properties)
                });
            }
            else return NotFound(new { message = "Suplier is not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
    [HttpGet]
    [Route("get/{id}")]
    public async Task<ActionResult> GetSupplierAsync(int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = @"
            MATCH (n:Supplier {id: $id})
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
            else return NotFound(new { message = "Supplier is not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
}