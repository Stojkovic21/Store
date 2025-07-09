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
    [Route("supplier/connect")]
    public async Task<ActionResult> ConnectRelationshipSupplierAsync([FromBody] RelationshipModel relationshipModel)
    {
        var query = @"
            MATCH (a:Item {id: $sourceId})
            MATCH (b:Supplier {id: $targetId})
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
    [Route("supplier/breakup")]
    public async Task<ActionResult> BreakupRelationshipSupplierAsync([FromBody] RelationshipModel relationshipModel)
    {
        var query = @"
            MATCH (a:Item {id: $sourceId})-[r:" + relationshipModel.RelationshipType + @"]->(b:Supplier {id: $targetId})
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