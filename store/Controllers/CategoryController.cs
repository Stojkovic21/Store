using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("[controller]")]
public class CategoryController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public CategoryController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        this.driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [Route("Dodaj")]
    [HttpPost]
    public async Task<ActionResult> AddCategotyAsync([FromBody] CategoryModel categoryModel)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();
            var testQuety = @"
            MATCH (n:Category {id: $id})
            RETURN n";
            var query = @"
            CREATE(n:Category {id:$id, naziv:$naziv})";
            var parameters = new Dictionary<string, object>
            {
                {"id",categoryModel.Id},
                {"naziv",categoryModel.Naziv}
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
    public async Task<ActionResult> GetCategoryAsync(int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = @"
            MATCH (n:Category {id: $id})
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
            else return NotFound(new { message = "Category is not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
    [HttpDelete]
    [Route("Delete/{id}")]
    public async Task<ActionResult> DeleteCategoryAsync(int id)
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
            MATCH (n:Category {id: $id})
            RETURN n";
            var deleteQuery = @"
            MATCH (n:Category {id: $id})
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
            return NotFound(new { message = "False", error = ex });
        }
    }
    [HttpPut]
    [Route("Put/{id}")]
    public async Task<ActionResult> EditCategoryAsync([FromBody] CategoryModel updateCategory, int id)
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
                        {"naziv", updateCategory.Naziv}
                    }
                }
            };
            var query = @"
            MATCH (n:Category {id: $id})
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
                    message = "Category updated seccessfully",
                    updatedCategoty = updatedNode.Properties
                });
            }
            else return NotFound(new { mesage = "Category not found" });
        }
        catch (System.Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
}
