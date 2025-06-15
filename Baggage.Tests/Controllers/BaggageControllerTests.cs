using Baggage.Controllers;
using Baggage.DTO.Requests;
using Baggage.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Contracts;
using Shared.Messages;

namespace Baggage.Tests.Controllers
{
    public class BaggageControllerTests
    {
        private static BaggageController Build(bool validDynamicId,
            out Mock<IBaggageService> svcMock,
            out Mock<IRequestClient<ValidateDynamicIdRequest>> sessionMock)
        {
            svcMock = new Mock<IBaggageService>();
            sessionMock = new Mock<IRequestClient<ValidateDynamicIdRequest>>();

            var sessionResp = new Mock<Response<ValidateDynamicIdResponse>>();
            sessionResp.SetupGet(x => x.Message).Returns(new ValidateDynamicIdResponse { IsValid = validDynamicId });
            sessionMock.Setup(x =>
                    x.GetResponse<ValidateDynamicIdResponse>(It.IsAny<ValidateDynamicIdRequest>(), default, default))
                .ReturnsAsync(sessionResp.Object);

            var logger = new Mock<ILogger<BaggageController>>();
            return new BaggageController(svcMock.Object, sessionMock.Object, logger.Object);
        }

        [Fact]
        public async Task GetAllowance_InvalidDynamicId_Returns_401()
        {
            var ctrl = Build(false, out _, out _);
            var res = await ctrl.GetAllowance("BAD", "O1", "P1");
            var obj = Assert.IsType<ObjectResult>(res.Result);
            Assert.Equal(401, obj.StatusCode);
        }

        [Fact]
        public async Task Register_Returns_Ok()
        {
            var ctrl = Build(true, out var svc, out _);
            svc.Setup(x => x.RegisterAsync("D1", "O1", "P1", 1, 10))
                .ReturnsAsync(new BaggageRegistrationDto { PassengerId = "P1" });

            var res = await ctrl.Register(new RegisterRequest
            {
                DynamicId = "D1",
                OrderId = "O1",
                PassengerId = "P1",
                Pieces = 1,
                WeightKg = 10
            });

            var ok = Assert.IsType<OkObjectResult>(res.Result);
            Assert.Equal("P1", ((BaggageRegistrationDto)ok.Value).PassengerId);
        }

        [Fact]
        public async Task Cancel_NotFound_Returns_404()
        {
            var ctrl = Build(true, out var svc, out _);
            svc.Setup(x => x.CancelAsync("D1", "O1", "P1")).ReturnsAsync(false);

            var res = await ctrl.Cancel(new CancelRequest { DynamicId = "D1", OrderId = "O1", PassengerId = "P1" });

            Assert.IsType<NotFoundResult>(res);
        }
    }
}