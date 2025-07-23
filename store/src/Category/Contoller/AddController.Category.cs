using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("category")]
public class AddCategoryController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    private readonly Neo4jQuery neo4JQuery;
    private const string CATEGORY = "Category";
    private const string RETURN = "RETURN";
    public AddCategoryController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        this.driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [Route("add")]
    [HttpPost]
    public async Task<ActionResult> AddCategotyAsync([FromBody] CategoryModel categoryModel)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();
            var testQuety = neo4JQuery.QueryByOneElement(CATEGORY,"id","id",RETURN);
            var query = @"
            CREATE(n:Category {id:$id, name:$name})";
            var parameters = new Dictionary<string, object>
            {
                {"id",categoryModel.Id},
                {"name",categoryModel.Name}
            };
            var result = await neo4JQuery.ExecuteReadAsync(session,testQuety,parameters);
            if (result == null)
            {
                var res = await neo4JQuery.ExecuteWriteAsync(session,query,parameters);
                return Ok("Nodes added successfully!");
            }
            else return NotFound(new { message = "Node existing" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}