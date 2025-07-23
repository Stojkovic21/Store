using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("category")]
public class DeleteCategoryController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    private readonly Neo4jQuery neo4JQuery;
    private const string CATEGORY = "Category";
    private const string RETURN = "RETURN";
    private const string DELETE = "DELETE";
    public DeleteCategoryController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        this.driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        neo4JQuery = new();
    }
    [HttpDelete]
    [Route("delete/{id}")]
    public async Task<ActionResult> DeleteCategoryAsync(int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var testQuety = neo4JQuery.QueryByOneElement(CATEGORY,"id","id",RETURN);
            var deleteQuery = neo4JQuery.QueryByOneElement(CATEGORY,"id","id",DELETE);
            var parameters = new Dictionary<string, object>
            {
                {"id",id}
            };
            var result = await neo4JQuery.ExecuteReadAsync(session,testQuety,parameters);
            if (result != null)
            {
                var res = await neo4JQuery.ExecuteWriteAsync(session,deleteQuery,parameters);
                return Ok("Nodes deleted successfully!");;
            }
            else return NotFound(new { message = "Not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
}