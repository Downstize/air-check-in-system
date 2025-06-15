using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SessionService.Controllers;
using SessionService.Services;

namespace SessionService.Tests.Controllers
{
    public class SessionControllerTests
    {
        private readonly Mock<ISessionsService> _svc = new();
        private readonly SessionController _ctrl;

        public SessionControllerTests()
        {
            var logger = Mock.Of<ILogger<SessionController>>();
            _ctrl = new SessionController(_svc.Object, logger);
        }

        [Fact]
        public async Task Register_ReturnsOk_AndCallsService()
        {
            var id = "D1";
            _svc.Setup(x => x.RegisterSessionAsync(id)).Returns(Task.CompletedTask).Verifiable();
            var result = await _ctrl.Register(id);
            Assert.IsType<OkResult>(result);
            _svc.Verify();
        }

        [Fact]
        public async Task Validate_ReturnsOk_WhenValid()
        {
            var id = "D2";
            _svc.Setup(x => x.ValidateSessionAsync(id)).ReturnsAsync(true);
            var result = await _ctrl.Validate(id);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Validate_ReturnsUnauthorized_WhenInvalid()
        {
            var id = "D3";
            _svc.Setup(x => x.ValidateSessionAsync(id)).ReturnsAsync(false);
            var result = await _ctrl.Validate(id);
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}