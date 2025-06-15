using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using PassengerService.Consumers;
using PassengerService.Data;
using PassengerService.Tests.Utilities;
using Shared.Messages;

namespace PassengerService.Tests.Consumers
{
    public class GetPassengerConsumerTests
    {
        private readonly ApplicationDbContext _db;
        private readonly GetPassengerConsumer _consumer;
        private readonly Mock<ConsumeContext<GetPassengerRequest>> _ctxMock;

        public GetPassengerConsumerTests()
        {
            _db = TestDbContextFactory.CreateInMemory();
            var loggerMock = new Mock<ILogger<GetPassengerConsumer>>();
            _consumer = new GetPassengerConsumer(_db, loggerMock.Object);

            _ctxMock = new Mock<ConsumeContext<GetPassengerRequest>>();
            _ctxMock.SetupGet(x => x.Message)
                .Returns(new GetPassengerRequest { PassengerId = _db.Passengers.First().PassengerId });
        }

        [Fact]
        public async Task Consume_NotFound_ThrowsKeyNotFound()
        {
            _ctxMock.SetupGet(x => x.Message)
                .Returns(new GetPassengerRequest { PassengerId = "NOPE" });

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _consumer.Consume(_ctxMock.Object));
        }

        [Fact]
        public async Task Consume_Found_RespondsWithDto()
        {
            GetPassengerResponse response = null!;
            _ctxMock
                .Setup(x => x.RespondAsync(It.IsAny<GetPassengerResponse>()))
                .Callback<GetPassengerResponse>(r => response = r)
                .Returns(Task.CompletedTask);

            await _consumer.Consume(_ctxMock.Object);

            Assert.NotNull(response);
            Assert.False(string.IsNullOrEmpty(response.Passenger.PassengerId));
        }
    }
}