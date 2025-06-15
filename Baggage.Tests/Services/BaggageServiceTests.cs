using Baggage.Clients;
using Baggage.Data;
using Baggage.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Contracts;
using Shared.Messages;

namespace Baggage.Tests.Services
{
    public class BaggageServiceTests
    {
        private static (BaggageService svc, ApplicationDbContext db) BuildService()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var db = new ApplicationDbContext(opts);

            var order = new OrderDto
            {
                OrderId = "O1",
                Passengers = new List<PassengerDto>
                {
                    new PassengerDto { PassengerId = "P1" }
                }
            };

            var orderResp = new Mock<Response<GetOrderResponse>>();
            orderResp.SetupGet(r => r.Message)
                .Returns(new GetOrderResponse { Order = order });

            var orderClient = new Mock<IRequestClient<GetOrderRequest>>();
            orderClient.Setup(c => c.GetResponse<GetOrderResponse>(
                    It.IsAny<GetOrderRequest>(), default, default))
                .ReturnsAsync(orderResp.Object);

            var passengerClient = new PassengerClient(
                orderClient.Object,
                new Mock<ILogger<PassengerClient>>().Object);

            var cache = new MemoryDistributedCache(
                Options.Create(new MemoryDistributedCacheOptions()));

            var svc = new BaggageService(
                db,
                passengerClient,
                new Mock<ILogger<BaggageService>>().Object,
                cache);

            return (svc, db);
        }


        [Fact]
        public async Task CancelAsync_Returns_False_When_Passenger_Not_Found()
        {
            var (svc, _) = BuildService();

            var result = await svc.CancelAsync("D1", "O1", "P2");

            Assert.False(result);
        }

        [Fact]
        public async Task RegisterAsync_Throws_When_Duplicate()
        {
            var (svc, db) = BuildService();

            db.PaidOptions.Add(new Models.PaidOption { Pieces = 1, WeightKg = 20, Price = 0 });
            db.BaggageRegistrations.Add(new Models.BaggageRegistration
            {
                RegistrationId = "R1",
                DynamicId = "D1",
                OrderId = "O1",
                PassengerId = "P1",
                Pieces = 1,
                WeightKg = 10,
                TransactionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            });
            db.SaveChanges();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.RegisterAsync("D1", "O1", "P1", 1, 10));
        }

        [Fact]
        public async Task GetAllowanceAsync_Caches_Response()
        {
            var (svc, db) = BuildService();

            db.PaidOptions.Add(new Models.PaidOption { Pieces = 1, WeightKg = 20, Price = 0 });
            db.SaveChanges();

            var first = await svc.GetAllowanceAsync("D1", "O1", "P1");
            var second = await svc.GetAllowanceAsync("D1", "O1", "P1");

            Assert.Equal(first.FreePieces, second.FreePieces);
            Assert.Equal(first.FreeWeightKg, second.FreeWeightKg);
        }
    }
}