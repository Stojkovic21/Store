using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("relationship")]
public class ItemSupplierRelationship : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    private readonly Relationsip createRelationship;

    public ItemSupplierRelationship(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        createRelationship = new Relationsip(driver);
    }
    [HttpPost]
    [Route("supplier/{sourceId}/{targetId}/{relationshipType}")]
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

        return await createRelationship.Relationship(query, parameters);
    }
    [HttpDelete]
    [Route("supplier/{sourceId}/{targetId}/{relationshipType}")]
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

        return await createRelationship.Relationship(query, parameters);
    }
}