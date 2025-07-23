using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("customer")]
public class EditCustomerController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    private readonly Neo4jQuery neo4JQuery;
    private const string CUSTOMER = "Customer";
    private const string RETURN = "RETURN";
    public EditCustomerController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        neo4JQuery = new();
    }
    //[Authorize(Roles = "Admin,User")]
    [HttpPut]
    [Route("edit")]
    public async Task<ActionResult> EditCustomerAsync([FromBody] KupacModel updateCustomer)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var findQuery = neo4JQuery.QueryByOneElement(CUSTOMER, "email", "email", RETURN);
            var findParameters = new Dictionary<string, object>
            {
                {"email",updateCustomer.Email}
            };
            var findResult = await neo4JQuery.ExecuteReadAsync(session,findQuery,findParameters);
            if (!(findResult != null))
            {
                return BadRequest(new
                {
                    message = "Enter correct password"
                });
            }
            
            updateCustomer.Ime = string.IsNullOrWhiteSpace(updateCustomer.Ime) ? findResult.Properties["ime"].ToString() : updateCustomer.Ime;
            updateCustomer.Prezime = string.IsNullOrWhiteSpace(updateCustomer.Prezime) ? findResult.Properties["prezime"].ToString() : updateCustomer.Prezime;
            updateCustomer.BrTel = string.IsNullOrWhiteSpace(updateCustomer.BrTel) ? findResult.Properties["brTel"].ToString() : updateCustomer.BrTel;
            updateCustomer.Role = string.IsNullOrWhiteSpace(updateCustomer.Role) ? findResult.Properties["role"].ToString() : updateCustomer.Role;
            updateCustomer.RefreshToken = string.IsNullOrWhiteSpace(updateCustomer.RefreshToken) ? findResult.Properties["refreshToken"].ToString() : updateCustomer.RefreshToken;
            updateCustomer.RefreshTokenTimeExpire = DateTime.UtcNow.AddDays(7);
            
            var parameters = new Dictionary<string, object>
            {
                {"email", updateCustomer.Email},
                {
                    "propertis",new Dictionary<string,object>{
                        {"ime",updateCustomer.Ime},
                        {"prezime",updateCustomer.Prezime},
                        {"brTel",updateCustomer.BrTel},
                        {"role",updateCustomer.Role},
                        {"refreshToken",updateCustomer.RefreshToken},
                        {"RTTimeExpire",updateCustomer.RefreshTokenTimeExpire}
                    }
                }
            };
            var query = @"
            MATCH (n:Customer {email: $email})
            SET n+=$propertis
            RETURN n";

            var updatedNode = await neo4JQuery.ExecuteWriteAsync(session,query,parameters);
            if (updatedNode != null)
            {
                return Ok(new
                {
                    message = "Customer updated seccessfully",
                    updatedCategoty = updatedNode.Properties
                });
            }
            else return NotFound(new { mesage = "Customer not found" });
        }
        catch (System.Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
}