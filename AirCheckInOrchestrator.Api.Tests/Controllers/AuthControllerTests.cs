using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using AirCheckInOrchestrator.Api.Controllers;

namespace AirCheckInOrchestrator.Api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly IConfiguration _config;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var key = new string('A', 32);
            var inMemory = new Dictionary<string, string?>
            {
                ["Auth:Username"] = "admin",
                ["Auth:Password"] = "secret",
                ["JwtSettings:Key"] = key,
                ["JwtSettings:Issuer"] = "issuer",
                ["JwtSettings:ExpireMinutes"] = "30"
            };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemory)
                .Build();
            _loggerMock = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_config, _loggerMock.Object);
        }

        [Fact]
        public void Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var result = _controller.Login("wrong", "creds");
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Неверный логин или пароль", unauthorized.Value);
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsJwtToken()
        {
            var result = _controller.Login("admin", "secret");
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = ok.Value!;
            var tokenProperty = value.GetType().GetProperty("token");
            Assert.NotNull(tokenProperty);
            var tokenString = tokenProperty.GetValue(value) as string;
            Assert.False(string.IsNullOrEmpty(tokenString));
        }
    }
}
