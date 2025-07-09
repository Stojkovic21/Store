using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Neo4j.Driver;

[ApiController]
[Route("customer")]
public class AuthCustomerController : ControllerBase
{
    private readonly IDriver driver;
    private readonly IConfiguration configuration;
    public AuthCustomerController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        this.driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }

    [Route("signup")]
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
                role: $role,
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
                {"role",kupacModel.Role},
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
                return Ok(CreateJWT(new JwtModel
                {
                    Email = kupacModel.Email,
                    Id = kupacModel.Id.ToString(),
                    Role = kupacModel.Role
                }));
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
                {"brTel",""},
                {"role",""}
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
                return Ok(CreateJWT(new JwtModel
                {
                    Email = result.Properties["email"]?.ToString(),
                    Id = result.Properties["id"]?.ToString(),
                    Role = result.Properties["role"]?.ToString()
                }));
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
    private string CreateJWT(JwtModel user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier,user.Id),
            new(ClaimTypes.Name,user.Email),
            new(ClaimTypes.Role,user.Role)   //Sve sto treba da bude u jwt tokenu se smesta u Claim
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("AppSettings:Issuer")!,
            audience: configuration.GetValue<string>("AppSettings:Audience")!,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}