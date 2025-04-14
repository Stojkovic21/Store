using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("[controller]")]
public class SupplierController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public SupplierController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [Route("Dodaj")]
    [HttpPost]
    public async Task<ActionResult> AddSupplierAsync([FromBody] SupplierModel supplierModel)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();
            var testQuety = @"
            MATCH (n:Supplier {id: $id})
            RETURN n";
            var query = @"
            CREATE(n:Supplier {id:$id, ime:$ime, email: $email})";
            var parameters = new Dictionary<string, object>
            {
                {"id",supplierModel.Id},
                {"ime",supplierModel.Ime},
                {"email",supplierModel.Email},
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
    [HttpGet]
    [Route("Get/{id}")]
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
    [HttpDelete]
    [Route("Delete/{id}")]
    public async Task<ActionResult> DeleteSupplierAsync(int id)
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
            MATCH (n:Supplier {id: $id})
            RETURN n";
            var deleteQuery = @"
            MATCH (n:Supplier {id: $id})
            DETACH DELETE n";
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
                var res = await session.ExecuteWriteAsync(async tx =>
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
