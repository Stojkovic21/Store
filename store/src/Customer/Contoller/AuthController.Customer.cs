using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
    private readonly Neo4jQuery neo4JQuery;
    private const string CUSTOMER = "Customer";
    private const string RETURN = "RETURN";
    public AuthCustomerController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var uri = this.configuration.GetValue<string>("Neo4j:Uri");
        var user = this.configuration.GetValue<string>("Neo4j:Username");
        var password = this.configuration.GetValue<string>("Neo4j:Password");

        this.driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        neo4JQuery = new();
    }

    [Route("signup")]
    [HttpPost]
    public async Task<ActionResult> SignUp([FromBody] KupacModel kupacModel)
    {
        var passwordHash = Argon2.Hash(kupacModel.Password);
        kupacModel.RefreshToken = GenerateRefreshToken();
        kupacModel.RefreshTokenTimeExpire = DateTime.UtcNow.AddDays(7);
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var testQuery = neo4JQuery.QueryByOneElement(CUSTOMER, "email", "email", RETURN);
            var createQuery = @"
                CREATE (n:Customer {
                    id: $id, 
                    email: $email,
                    password: $password,
                    role: $role,
                    ime: $ime, 
                    prezime: $prezime, 
                    brTel: $brTel,
                    refreshToken: $refreshToken,
                    RTTimeExpire: $RTTimeExpire
                })
                RETURN n";

            var parameters = new Dictionary<string, object>
            {
                {"id", kupacModel.Id},
                {"password", passwordHash},
                {"email", kupacModel.Email},
                {"ime", kupacModel.Ime},
                {"prezime", kupacModel.Prezime},
                {"role", kupacModel.Role},
                {"brTel", kupacModel.BrTel},
                {"refreshToken", ""},
                {"RTTimeExpire", kupacModel.RefreshTokenTimeExpire}
            };

            var existingCustomer = await neo4JQuery.ExecuteReadAsync(session, testQuery, parameters);

            if (existingCustomer != null)
            {
                return Conflict(new { message = "Customer already exists" });
            }

            var newCustomer = await neo4JQuery.ExecuteWriteAsync(session, createQuery, parameters);

            if (newCustomer != null)
            {
                var refreshToken = await GenerateAndSaveRefreshTokenAsync(new LoginModel(kupacModel.Email, kupacModel.Password));
                Response.Cookies.Append("refreshToken", kupacModel.RefreshToken.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
                return Ok(
                    new ResponseTokenModel
                    {
                        AccessToken = CreateJWT(new JwtModel
                        {
                            Email = newCustomer.Properties["email"]?.ToString(),
                            UserId = newCustomer.Properties["id"]?.ToString(),
                            Role = newCustomer.Properties["role"]?.ToString()
                        }),
                        UserId = int.Parse(newCustomer.Properties["id"].ToString()),
                        Role = newCustomer.Properties["role"]?.ToString()
                    }
                );
            }
            return StatusCode(500, new { message = "Failed to create customer" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
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
            var testQuety = neo4JQuery.QueryByOneElement(CUSTOMER, "email", "email", RETURN);
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
            var result = await neo4JQuery.ExecuteReadAsync(session, testQuety, parameters);
            if (result != null && Argon2.Verify(result.Properties["password"]?.ToString(), loginModel.Password))
            {
                var refreshToken = await GenerateAndSaveRefreshTokenAsync(loginModel);
                Response.Cookies.Append("refreshToken", refreshToken.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
                return Ok(new ResponseTokenModel
                {
                    AccessToken = CreateJWT(new JwtModel
                    {
                        Email = result.Properties["email"]?.ToString(),
                        UserId = result.Properties["id"]?.ToString(),
                        Role = result.Properties["role"]?.ToString()
                    }),
                    UserId = int.Parse(result.Properties["id"].ToString()),
                    Role = result.Properties["role"]?.ToString(),
                });
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
            new(ClaimTypes.NameIdentifier,user.UserId),
            new(ClaimTypes.Name,user.Email),
            new(ClaimTypes.Role,user.Role)   //Sve sto treba da bude u jwt tokenu se smesta u Claim
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("AppSettings:Issuer")!,
            audience: configuration.GetValue<string>("AppSettings:Audience")!,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(LoginModel loginData)
    {
        var refreshToken = GenerateRefreshToken();
        var context = new EditCustomerController(configuration);
        await context.EditCustomerAsync(new KupacModel
        {
            RefreshToken = refreshToken,
            RefreshTokenTimeExpire = DateTime.UtcNow.AddDays(7),
            Email = loginData.Email,
            Password = loginData.Password,
            Role = "",
            Ime = "",
            Prezime = "",
            BrTel = ""
        });
        return refreshToken;
    }
    [HttpGet]
    [Route("refresh-token")]
    public async Task<ActionResult> ValidateRefreshTokenAsync()
    {
        string refreshToken = Request.Cookies["refreshToken"];
        Console.WriteLine(refreshToken);
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = neo4JQuery.QueryByOneElement(CUSTOMER, "refreshToken", "refreshToken", RETURN);
            var parameters = new Dictionary<string, object>
            {
                {"refreshToken",refreshToken}
            };
            var result = await neo4JQuery.ExecuteReadAsync(session, query, parameters);
            Console.WriteLine(result.Properties["ime"]);
            var dateNow = DateTime.UtcNow.ToLocalTime();
            var dateParse = DateTime.Parse(result.Properties["RTTimeExpire"].ToString()).ToLocalTime();
            if (result is not null && dateNow < dateParse)
            {
                var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(new LoginModel(result.Properties["email"].ToString(), result.Properties["password"].ToString()));
                Response.Cookies.Append("refreshToken", newRefreshToken.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
                return Ok(new ResponseTokenModel
                {
                    AccessToken = CreateJWT(new JwtModel
                    {
                        Email = result.Properties["email"]?.ToString(),
                        UserId = result.Properties["id"]?.ToString(),
                        Role = result.Properties["role"]?.ToString()
                    }),
                    UserId = int.Parse(result.Properties["id"]?.ToString()),
                    Role = result.Properties["role"]?.ToString(),
                });
            }
            return BadRequest("Refresh tokeh has expire!"); //ponovno logovanje
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
    [HttpGet]
    [Route("signout")]
    public void CustomerSignOut()
    {
        string newRefreshToken = "";
        Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }
}