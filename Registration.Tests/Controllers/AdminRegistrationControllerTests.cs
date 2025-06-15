using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Registration.Controllers;
using Registration.Models;
using Registration.Services;

namespace Registration.Tests.Controllers
{
    public class AdminRegistrationControllerTests
    {
        private readonly Mock<IRegistrationService> _svc = new();
        private readonly Mock<ILogger<AdminRegistrationController>> _logger = new();

        private AdminRegistrationController Create() =>
            new AdminRegistrationController(_svc.Object, _logger.Object);

        [Fact]
        public async Task GetPayments_ReturnsOk_WithList()
        {
            var list = new List<Payment> { new() { PaymentId = 1 } };
            _svc.Setup(x => x.GetAllPaymentsAsync()).ReturnsAsync(list);

            var ctrl = Create();
            var ok = await ctrl.GetPayments() as OkObjectResult;

            Assert.NotNull(ok);
            var result = Assert.IsAssignableFrom<IEnumerable<Payment>>(ok.Value);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetPayment_ReturnsNotFound_WhenNull()
        {
            _svc.Setup(x => x.GetPaymentByIdAsync(42)).ReturnsAsync((Payment)null);

            var ctrl = Create();
            var result = await ctrl.GetPayment(42);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreatePayment_ReturnsCreated()
        {
            var p = new Payment { PassengerId = "P", PaymentId = 2 };
            _svc.Setup(x => x.CreatePaymentAsync(p)).ReturnsAsync(p);

            var ctrl = Create();
            var created = await ctrl.CreatePayment(p) as CreatedAtActionResult;

            Assert.NotNull(created);
            var returned = Assert.IsType<Payment>(created.Value);
            Assert.Equal(2, returned.PaymentId);
        }
    }
}