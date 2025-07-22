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
    public async Task<ActionResult> SignUp([FromBody] KupacModel kupacModel)
    {
        var passwordHash = Argon2.Hash(kupacModel.Password);
        kupacModel.RefreshToken = GenerateRefreshToken();
        kupacModel.RefreshTokenTimeExpire = DateTime.UtcNow.AddDays(7);
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var testQuery = @"
                MATCH (n:Customer {email: $email})
                RETURN n";

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

            var existingCustomer = await session.ExecuteReadAsync(async tx =>
            {
                var response = await tx.RunAsync(testQuery, parameters);
                if (await response.FetchAsync())
                {
                    return response.Current["n"].As<INode>();
                }
                return null;
            });

            if (existingCustomer != null)
            {
                return Conflict(new { message = "Customer already exists" });
            }

            var newCustomer = await session.ExecuteWriteAsync(async tx =>
            {
                var response = await tx.RunAsync(createQuery, parameters);
                if (await response.FetchAsync())
                {
                    return response.Current["n"].As<INode>();
                }
                return null;
            });

            if (newCustomer == null)
            {
                return StatusCode(500, new { message = "Failed to create customer" });
            }

            return Ok(
                new ResponseTokenModel
                {
                    AccessToken = CreateJWT(new JwtModel
                    {
                        Email = newCustomer.Properties["email"]?.ToString(),
                        Id = newCustomer.Properties["id"]?.ToString(),
                        Role = newCustomer.Properties["role"]?.ToString()
                    }),
                    RefreshToken = await GenerateAndSaveRefreshTokenAsync(new LoginModel(kupacModel.Email, kupacModel.Password))
                }
            );
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
                        Id = result.Properties["id"]?.ToString(),
                        Role = result.Properties["role"]?.ToString()
                    }),
                    RefreshToken = refreshToken //ovo ne bi trebalo da salje na front
                    // User = new KupacModel
                    // {
                    //     Ime = result.Properties["ime"].ToString(),
                    //     Prezime = "",
                    //     Email = loginData.Email,
                    //     Password = loginData.Password,
                    //     Role = "",
                    //     BrTel = ""
                    //     RefreshToken = refreshToken,
                    //     RefreshTokenTimeExpire = DateTime.UtcNow.AddDays(7),
                    // }
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
        var refreshToken = Request.Cookies["refreshToken"];
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

            if (result is null || result.Properties["refreshToken"].ToString() != refreshToken)
            {
                return BadRequest("The refresh token has expire");
            }
            return Ok(new ResponseTokenModel
            {
                AccessToken = CreateJWT(new JwtModel
                {
                    Email = result.Properties["email"]?.ToString(),
                    Id = result.Properties["id"]?.ToString(),
                    Role = result.Properties["role"]?.ToString()
                }),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(new LoginModel(result.Properties["email"].ToString(), result.Properties["password"].ToString()))
            });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "False", error = ex });
        }
    }
}