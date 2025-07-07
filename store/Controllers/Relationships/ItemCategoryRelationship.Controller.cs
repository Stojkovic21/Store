using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("relationship")]
public class ItemCategoryRelationship : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    private readonly Relationsip createRelationship;
    public ItemCategoryRelationship(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        createRelationship = new Relationsip(driver);
    }

    [HttpPost]
    [Route("category/{sourceId}/{targetId}/{relationshipType}")]
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

        return await createRelationship.Relationship(query, parameters);
    }
    [HttpDelete]
    [Route("category/{sourceId}/{targetId}/{relationshipType}")]
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

        return await createRelationship.Relationship(query, parameters);
    }
}