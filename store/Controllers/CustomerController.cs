using System.Net.Http.Json;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;


[ApiController]
[Route("[controller]")]
public class CustomerController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public CustomerController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [Route("Dodaj")]
    [HttpPost]
    public async Task<ActionResult> AddCustomerAsync([FromBody] KupacModel kupacModel)
    {
        var passwordHash = Argon2.Hash(kupacModel.Password);
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();
            var testQuety = @"
            MATCH (n:Customer {email: $email})
            RETURN n";
            var query = @"
            CREATE (n:Customer {
                id: $id, 
                email: $email,
                password: $password,
                ime: $ime, 
                prezime: $prezime, 
                brTel: $brTel
            })";
            var parameters = new Dictionary<string, object>
            {
                {"id",kupacModel.Id},
                {"password",passwordHash},
                {"email",kupacModel.Email},
                {"ime",kupacModel.Ime},
                {"prezime",kupacModel.Prezime},
                {"brTel",kupacModel.BrTel}
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
    [HttpPost]
    [Route("login")]
    public async Task<ActionResult> Login([FromBody] LoginModel loginModel)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();
            var testQuety = @"
                MATCH (n:Customer {email: $email})
                RETURN n";
            var parameters = new Dictionary<string, object>
            {
                {"id",""},
                {"password",loginModel.Password},
                {"email",loginModel.Email},
                {"ime",""},
                {"prezime",""},
                {"brTel",""}
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

            if (result != null && Argon2.Verify(result.Properties["password"]?.ToString(), loginModel.Password))
            {
                return Ok(result.Properties);
            }
            return BadRequest(new
            {
                message = "Not correct email or password"
            });
        }
        catch (System.Exception)
        {
            throw;
        }
    }
    [HttpGet]
    [Route("Get/all")]
    public async Task<ActionResult> GetAllCustomerAsync()
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = @"
            MATCH (n:Customer)
            RETURN n";
            var parameters = new Dictionary<string, object>
            {
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
                    customer = result.Properties
                });
            }
            else return NotFound(new { message = "Customer is not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
    [HttpGet]
    [Route("Get/{id}")]
    public async Task<ActionResult> GetCustomerAsync(int id)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = @"
            MATCH (n:Customer {id: $id})
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
            else return NotFound(new { message = "Customer is not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
    [HttpDelete]
    [Route("Delete/{id}")]
    public async Task<ActionResult> DeleteCustomerAsync(int id)
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
            MATCH (n:Customer {id: $id})
            RETURN n";
            var deleteQuery = @"
            MATCH (n:Customer {id: $id})
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
    public async Task<ActionResult> EditCustomerAsync([FromBody] KupacModel updateCustomer, int id)
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
                        {"ime",updateCustomer.Ime},
                        {"prezime",updateCustomer.Prezime},
                        {"brTel",updateCustomer.BrTel},
                        {"email",updateCustomer.Email}
                    }
                }
            };
            var query = @"
            MATCH (n:Customer {id: $id})
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
    // [HttpPost]
    // [Route("Relationship/{sourceId}/{targetId}/{relationshipType}")]
    // public async Task<ActionResult> CreateRelationshipAsync(int sourceId, int targetId, string relationshipType)
    // {
    //     var query = @"
    //         MATCH (a:Customer {id: $sourceId})
    //         MATCH (b:Order {id: $targetId})
    //         MERGE (a)-[r:" + relationshipType + @"]->(b)
    //         RETURN a, r, b";

    //     // Create parameters for the query
    //     var parameters = new Dictionary<string, object>
    //     {
    //         { "sourceId", sourceId },
    //         { "targetId", targetId }
    //     };

    //     return await Relationship(query,parameters);
    // }
    // [HttpDelete]
    // [Route("Relationship/{sourceId}/{targetId}/{relationshipType}")]
    // public async Task<ActionResult> DeleteRelationshipAsync(int sourceId, int targetId, string relationshipType)
    // {
    //     var query = @"
    //         MATCH (a:Customer {id: $sourceId})-[r:" + relationshipType + @"]->(b:Order {id: $targetId})
    //         DELETE r
    //         RETURN a, b";

    //     // Create parameters for the query
    //     var parameters = new Dictionary<string, object>
    //     {
    //         { "sourceId", sourceId },
    //         { "targetId", targetId }
    //     };

    //     return await Relationship(query,parameters);
    // }
    // private async Task<ActionResult> Relationship(string query, Dictionary<string, object> parameters)
    // {
    //     try
    //     {
    //         await driver.VerifyConnectivityAsync();
    //         await using var session=driver.AsyncSession();
    //         // Execute the query in a write transaction
    //         var result = await session.ExecuteWriteAsync(async tx =>
    //         {
    //             var response = await tx.RunAsync(query, parameters);

    //             // Check if the query returned any result
    //             if (await response.FetchAsync())
    //             {
    //                 return new
    //                 {
    //                     Source = response.Current["a"].As<INode>().Properties,
    //                     Target = response.Current["b"].As<INode>().Properties
    //                 };
    //             }

    //             return null;
    //         });

    //         if (result != null)
    //         {
    //             return Ok(new
    //             {
    //                 message = "Relationship deleted successfully!",
    //                 nodes = result
    //             });
    //         }
    //         else
    //         {
    //             return NotFound(new { message = "Relationship or nodes not found!" });
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"An error occurred: {ex.Message}");
    //         return StatusCode(500, new { message = "Internal server error", error = ex.Message });
    //     }
    // }
}
