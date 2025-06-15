using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using PassengerService.Consumers;
using PassengerService.Data;
using PassengerService.Tests.Utilities;
using Shared.Messages;

namespace PassengerService.Tests.Consumers
{
    public class GetOrderConsumerTests
    {
        private readonly ApplicationDbContext _db;
        private readonly GetOrderConsumer _consumer;
        private readonly Mock<ConsumeContext<GetOrderRequest>> _ctxMock;

        public GetOrderConsumerTests()
        {
            _db = TestDbContextFactory.CreateInMemory();
            var loggerMock = new Mock<ILogger<GetOrderConsumer>>();
            _consumer = new GetOrderConsumer(_db, loggerMock.Object);

            _ctxMock = new Mock<ConsumeContext<GetOrderRequest>>();
            _ctxMock.SetupGet(x => x.Message).Returns(new GetOrderRequest
            {
                OrderId = "AB1234",
                DynamicId = "D1",
                LastName = "Ivanov"
            });
        }

        [Fact]
        public async Task Consume_NotFound_RespondsWithNullOrder()
        {
            _ctxMock.SetupGet(x => x.Message).Returns(new GetOrderRequest { OrderId = "NOPE" });

            GetOrderResponse response = null!;
            _ctxMock
                .Setup(x => x.RespondAsync(It.IsAny<GetOrderResponse>()))
                .Callback<GetOrderResponse>(r => response = r)
                .Returns(Task.CompletedTask);

            await _consumer.Consume(_ctxMock.Object);

            Assert.NotNull(response);
            Assert.Null(response.Order);
        }

        [Fact]
        public async Task Consume_Found_RespondsWithOrderDto()
        {
            GetOrderResponse response = null!;
            _ctxMock
                .Setup(x => x.RespondAsync(It.IsAny<GetOrderResponse>()))
                .Callback<GetOrderResponse>(r => response = r)
                .Returns(Task.CompletedTask);

            await _consumer.Consume(_ctxMock.Object);
            
            Assert.NotNull(response);
            Assert.Equal("AB1234", response.Order.OrderId);
            Assert.NotEmpty(response.Order.Passengers);
            Assert.Single(response.Order.Segments);
        }
    }
}