using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("[controller]")]
public class EditCustomerController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public EditCustomerController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPut]
    [Route("Edit")]
    public async Task<ActionResult> EditCustomerAsync([FromBody] KupacModel updateCustomer)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var findQuery = @"
            MATCH (n:Customer {email: $email})
            RETURN n;
            ";
            var findParameters = new Dictionary<string, object>
            {
                {"email",updateCustomer.Email}
            };
            var findResult = await session.ExecuteReadAsync(async tx =>
            {
                var response = await tx.RunAsync(findQuery, findParameters);
                if (await response.FetchAsync())
                {
                    return response.Current["n"].As<INode>();
                }
                return null;
            });
            //Console.WriteLine(findResult.Properties);
            updateCustomer.Ime = updateCustomer.Ime == "" ? findResult.Properties["ime"].ToString() : updateCustomer.Ime;
            updateCustomer.Prezime = updateCustomer.Prezime == "" ? findResult.Properties["prezime"].ToString() : updateCustomer.Prezime;
            updateCustomer.BrTel = updateCustomer.BrTel == "" ? findResult.Properties["brTel"].ToString() : updateCustomer.BrTel;
            updateCustomer.Role = updateCustomer.Role == "" ? findResult.Properties["role"].ToString() : updateCustomer.Role;

            if (!(findResult != null && Argon2.Verify(findResult.Properties["password"]?.ToString(), updateCustomer.Password)))
            {
                return BadRequest(new
                {
                    message = "Enter correct password"
                });
            }

            var parameters = new Dictionary<string, object>
            {
                {"email", updateCustomer.Email},
                {
                    "propertis",new Dictionary<string,object>{
                        {"ime",updateCustomer.Ime},
                        {"prezime",updateCustomer.Prezime},
                        {"brTel",updateCustomer.BrTel},
                        {"role",updateCustomer.Role}
                    }
                }
            };
            var query = @"
            MATCH (n:Customer {email: $email})
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