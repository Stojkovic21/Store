using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("relationship")]
public class OrderItemRelationship : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    private readonly Relationsip createRelationship;

    public OrderItemRelationship(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        createRelationship = new Relationsip(driver);
    }
    [HttpPost]
    [Route("item/connect")]
    public async Task<ActionResult> ConnectRelationshiItemAsync([FromBody] RelationshipModel relationshipModel)
    {
        var query = @"
            MATCH (a:Order {id: $sourceId})
            MATCH (b:Item {id: $targetId})
            MERGE (a)-[r:" + relationshipModel.RelationshipType + @"]->(b)
            RETURN a, r, b";

        // Create parameters for the query
        var parameters = new Dictionary<string, object>
        {
            { "sourceId", relationshipModel.SourceId },
            { "targetId", relationshipModel.TargetId }
        };

        return await createRelationship.Relationship(query, parameters);
    }
    [HttpDelete]
    [Route("item/breakup")]
    public async Task<ActionResult> BreakupRelationshipItemAsync([FromForm] RelationshipModel relationshipModel)
    {
        var query = @"
            MATCH (a:Order {id: $sourceId})-[r:" + relationshipModel.RelationshipType + @"]->(b:Item {id: $targetId})
            DELETE r
            RETURN a, b";

        // Create parameters for the query
        var parameters = new Dictionary<string, object>
        {
            { "sourceId", relationshipModel.SourceId },
            { "targetId", relationshipModel.TargetId }
        };

        return await createRelationship.Relationship(query, parameters);
    }
}