using Baggage.Controllers;
using Baggage.Models;
using Baggage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Baggage.Tests.Controllers
{
    public class AdminBaggageControllerTests
    {
        private static AdminBaggageController Build(out Mock<IBaggageService> svcMock)
        {
            svcMock = new Mock<IBaggageService>();
            var logger = new Mock<ILogger<AdminBaggageController>>();
            return new AdminBaggageController(svcMock.Object, logger.Object);
        }

        [Fact]
        public async Task GetAllPayments_Returns_List()
        {
            var ctrl = Build(out var svc);
            var list = new List<BaggagePayment> { new BaggagePayment { PaymentId = "1" } };
            svc.Setup(x => x.GetAllPaymentsAsync()).ReturnsAsync(list);

            var res = await ctrl.GetAllPayments();

            var ok = Assert.IsType<OkObjectResult>(res.Result);
            Assert.Equal(list, ok.Value);
        }

        [Fact]
        public async Task GetPaymentById_NotFound_Returns_NotFound()
        {
            var ctrl = Build(out var svc);
            svc.Setup(x => x.GetPaymentByIdAsync("X")).ReturnsAsync((BaggagePayment)null);

            var res = await ctrl.GetPaymentById("X");

            Assert.IsType<NotFoundResult>(res.Result);
        }

        [Fact]
        public async Task CreatePayment_Returns_Ok_With_Id()
        {
            var ctrl = Build(out var svc);
            var pay = new BaggagePayment { PassengerId = "P1", Amount = 10 };
            svc.Setup(x => x.CreatePaymentAsync(It.IsAny<BaggagePayment>()))
                .ReturnsAsync((BaggagePayment p) =>
                {
                    p.PaymentId = "NEW";
                    return p;
                });

            var res = await ctrl.CreatePayment(pay);

            var ok = Assert.IsType<OkObjectResult>(res.Result);
            Assert.Equal("NEW", ((BaggagePayment)ok.Value).PaymentId);
        }
    }
}