using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AirCheckInOrchestrator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration config, ILogger<AuthController> logger)
    {
        _config = config;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login(string username, string password)
    {
        _logger.LogInformation("Попытка входа пользователя {Username}", username);
        
        var expectedUsername = _config["Auth:Username"];
        var expectedPassword = _config["Auth:Password"];

        if (username != expectedUsername || password != expectedPassword)
        {
            _logger.LogWarning("Неудачная попытка входа пользователя {Username}", username);
            return Unauthorized("Неверный логин или пароль");
        }

        var jwtSettings = _config.GetSection("JwtSettings");
        var key = jwtSettings["Key"];
        var issuer = jwtSettings["Issuer"];
        var expireMinutes = int.Parse(jwtSettings["ExpireMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: null,
            claims: claims,
            expires: DateTime.Now.AddMinutes(expireMinutes),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogInformation("Пользователь {Username} успешно вошёл в систему", username);

        return Ok(new { token = tokenString });
    }
}