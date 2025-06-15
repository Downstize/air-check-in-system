using Baggage.Consumers;
using Baggage.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Messages;

namespace Baggage.Tests.Consumers
{
    public class GetPassengerBaggageWeightConsumerTests
    {
        private static ApplicationDbContext Seed()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new ApplicationDbContext(opts);

            db.BaggageRegistrations.AddRange(
                new Models.BaggageRegistration
                {
                    RegistrationId = "R1",
                    DynamicId = "D1",
                    OrderId = "O1",
                    PassengerId = "P1",
                    WeightKg = 10,
                    Pieces = 1,
                    TransactionId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                },
                new Models.BaggageRegistration
                {
                    RegistrationId = "R2",
                    DynamicId = "D1",
                    OrderId = "O1",
                    PassengerId = "P1",
                    WeightKg = 5,
                    Pieces = 1,
                    TransactionId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                });

            db.SaveChanges();
            return db;
        }

        private static Mock<IRequestClient<ValidateDynamicIdRequest>> SessionClient(bool valid)
        {
            var resp = new Mock<Response<ValidateDynamicIdResponse>>();
            resp.SetupGet(r => r.Message).Returns(new ValidateDynamicIdResponse { IsValid = valid });

            var client = new Mock<IRequestClient<ValidateDynamicIdRequest>>();
            client.Setup(c => c.GetResponse<ValidateDynamicIdResponse>(
                    It.IsAny<ValidateDynamicIdRequest>(), default, default))
                .ReturnsAsync(resp.Object);
            return client;
        }

        [Fact]
        public async Task Consume_Valid_Request_Returns_Sum()
        {
            var db = Seed();
            var logger = new Mock<ILogger<GetPassengerBaggageWeightConsumer>>();
            var consumer = new GetPassengerBaggageWeightConsumer(
                db, SessionClient(true).Object, logger.Object);

            var ctx = new Mock<ConsumeContext<GetPassengerBaggageWeightRequest>>();
            ctx.SetupGet(c => c.Message).Returns(new GetPassengerBaggageWeightRequest
            {
                DynamicId = "D1",
                OrderId = "O1",
                PassengerId = "P1"
            });

            await consumer.Consume(ctx.Object);

            ctx.Verify(c => c.RespondAsync(
                    It.Is<GetPassengerBaggageWeightResponse>(r => r.TotalWeightKg == 15)),
                Times.Once);
        }

        [Fact]
        public async Task Consume_InvalidDynamicId_Returns_Zero()
        {
            var db = Seed();
            var logger = new Mock<ILogger<GetPassengerBaggageWeightConsumer>>();
            var consumer = new GetPassengerBaggageWeightConsumer(
                db, SessionClient(false).Object, logger.Object);

            var ctx = new Mock<ConsumeContext<GetPassengerBaggageWeightRequest>>();
            ctx.SetupGet(c => c.Message).Returns(new GetPassengerBaggageWeightRequest
            {
                DynamicId = "BAD",
                OrderId = "O1",
                PassengerId = "P1"
            });

            await consumer.Consume(ctx.Object);

            ctx.Verify(c => c.RespondAsync(
                    It.Is<GetPassengerBaggageWeightResponse>(r => r.TotalWeightKg == 0)),
                Times.Once);
        }
    }
}