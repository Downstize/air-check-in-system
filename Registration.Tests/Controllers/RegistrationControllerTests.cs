using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Registration.Controllers;
using Registration.DTO.Auth;
using Registration.DTO.Order;
using Registration.Models;
using Registration.Services;
using Shared.Contracts;

namespace Registration.Tests.Controllers
{
    public class RegistrationControllerTests
    {
        private readonly Mock<IRegistrationService> _svc = new();
        private readonly RegistrationAuthOptions _optsValue = new() { Login = "u", Password = "p" };
        private readonly Mock<ILogger<RegistrationController>> _logger = new();

        private RegistrationController Create()
        {
            var opts = Mock.Of<IOptions<RegistrationAuthOptions>>(o => o.Value == _optsValue);
            return new RegistrationController(_svc.Object, opts, _logger.Object);
        }

        [Fact]
        public async Task Authenticate_ReturnsUnauthorized_OnBadCreds()
        {
            var ctrl = Create();
            var bad = new RegistrationAuthRequestDto { Login = "x", Pwd = "y" };
            var actionResult = await ctrl.Authenticate(bad);
            Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task Authenticate_ReturnsOk_OnGoodCreds()
        {
            var expectedDto = new RegistrationAuthResponseDto { DynamicId = "d" };
            _svc.Setup(s => s.AuthenticateAsync(It.IsAny<RegistrationAuthRequestDto>()))
                .ReturnsAsync(expectedDto);

            var ctrl = Create();
            var actionResult = await ctrl.Authenticate(new RegistrationAuthRequestDto { Login = "u", Pwd = "p" });

            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returned = Assert.IsType<RegistrationAuthResponseDto>(ok.Value);
            Assert.Equal(expectedDto.DynamicId, returned.DynamicId);
        }

        [Fact]
        public async Task SearchOrder_ReturnsOk_WhenServiceReturnsData()
        {
            var expectedDto = new RegistrationOrderSearchResponseDto
            {
                Order = new OrderDto { OrderId = "PNR1" }
            };
            _svc.Setup(s => s.SearchOrderAsync(It.IsAny<RegistrationOrderSearchRequestDto>()))
                .ReturnsAsync(expectedDto);

            var ctrl = Create();
            var req = new RegistrationOrderSearchRequestDto
            {
                DynamicId = "dyn",
                PnrId = "PNR1",
                LastName = "Ivanov"
            };

            var actionResult = await ctrl.SearchOrder(req);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returned = Assert.IsType<RegistrationOrderSearchResponseDto>(ok.Value);
            Assert.Same(expectedDto, returned);
        }
    }
}