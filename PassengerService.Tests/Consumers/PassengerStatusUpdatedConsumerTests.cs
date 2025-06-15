using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using PassengerService.Consumers;
using PassengerService.Data;
using PassengerService.Tests.Utilities;
using Shared.Messages;

namespace PassengerService.Tests.Consumers
{
    public class PassengerStatusUpdatedConsumerTests
    {
        private readonly ApplicationDbContext _db;
        private readonly PassengerStatusUpdatedConsumer _consumer;
        private readonly Mock<ConsumeContext<PassengerStatusUpdated>> _ctxMock;

        public PassengerStatusUpdatedConsumerTests()
        {
            _db = TestDbContextFactory.CreateInMemory();
            var loggerMock = new Mock<ILogger<PassengerStatusUpdatedConsumer>>();
            _consumer = new PassengerStatusUpdatedConsumer(_db, loggerMock.Object);

            var existingId = _db.Passengers.First().PassengerId;
            _ctxMock = new Mock<ConsumeContext<PassengerStatusUpdated>>();
            _ctxMock.SetupGet(x => x.Message)
                .Returns(new PassengerStatusUpdated { PassengerId = existingId, NewStatus = "checked" });
        }

        [Fact]
        public async Task Consume_UpdatesStatus_WhenExists()
        {
            await _consumer.Consume(_ctxMock.Object);
            var p = _db.Passengers.First();
            Assert.Equal("checked", p.CheckInStatus);
        }

        [Fact]
        public async Task Consume_Ignores_WhenNotExists()
        {
            _ctxMock.SetupGet(x => x.Message)
                .Returns(new PassengerStatusUpdated { PassengerId = "NOPE", NewStatus = "X" });

            await _consumer.Consume(_ctxMock.Object);
            Assert.NotEmpty(_db.Passengers);
        }
    }
}