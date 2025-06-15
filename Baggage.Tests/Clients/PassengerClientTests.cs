using Baggage.Clients;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Contracts;
using Shared.Messages;

namespace Baggage.Tests.Clients
{
    public class PassengerClientTests
    {
        private static PassengerClient Build(out Mock<IRequestClient<GetOrderRequest>> clientMock)
        {
            clientMock = new Mock<IRequestClient<GetOrderRequest>>();
            var logger = new Mock<ILogger<PassengerClient>>();
            return new PassengerClient(clientMock.Object, logger.Object);
        }

        [Fact]
        public async Task GetOrderAsync_Returns_OrderDto_When_Successful()
        {
            var svc = Build(out var clientMock);
            var order = new OrderDto { OrderId = "O1" };

            var resp = new Mock<Response<GetOrderResponse>>();
            resp.SetupGet(x => x.Message).Returns(new GetOrderResponse { Order = order });

            clientMock.Setup(c => c.GetResponse<GetOrderResponse>(
                    It.IsAny<GetOrderRequest>(), default, default))
                .ReturnsAsync(resp.Object);

            var result = await svc.GetOrderAsync("D1", "O1", "Smith");

            Assert.Equal(order, result);
        }

        [Fact]
        public async Task GetOrderAsync_Throws_RequestTimeoutException()
        {
            var svc = Build(out var clientMock);

            clientMock
                .Setup(c => c.GetResponse<GetOrderResponse>(
                    It.IsAny<GetOrderRequest>(),
                    default,
                    default))
                .ThrowsAsync(new RequestTimeoutException("симуляция таймаута"));

            await Assert.ThrowsAsync<RequestTimeoutException>(() => svc.GetOrderAsync("D1", "O1", "Smith"));
        }

        [Fact]
        public async Task GetOrderAsync_Throws_Generic_Exception()
        {
            var svc = Build(out var clientMock);

            clientMock.Setup(c => c.GetResponse<GetOrderResponse>(
                    It.IsAny<GetOrderRequest>(), default, default))
                .ThrowsAsync(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => svc.GetOrderAsync("D1", "O1", "Smith"));
        }
    }
}