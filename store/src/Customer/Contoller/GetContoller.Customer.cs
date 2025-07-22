using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

[ApiController]
[Route("customer")]
public class GetCustomerController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public GetCustomerController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        this.driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }
    [Authorize]
    [HttpGet]
    [Route("get/all")]
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
                var customers = new List<INode>();
                while (await response.FetchAsync())
                {
                    customers.Add(response.Current["n"].As<INode>());
                }
                return customers;
            });
            if (result != null)
            {
                return Ok(new
                {
                    message = "True",
                    customer = result.Select(s => s.Properties)
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
    [Route("{id}")]
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
                    Customer = result.Properties
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
    [Route("me")]
    public async Task<ActionResult> GetMe()
    {
        string refreshToken = Request.Cookies["refreshToken"].ToString();
        
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = @"
            MATCH (n:Customer {refreshToken: $refreshToken})
            RETURN n";
            var parameters = new Dictionary<string, object>
            {
                {"refreshToken",refreshToken}
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
                    Customer = result.Properties
                });
            }
            else return NotFound(new { message = "Customer is not found" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
}
