using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Registration.Clients;
using Shared.Contracts;
using Shared.Messages;

namespace Registration.Tests.Clients
{
    public class PassengerClientTests
    {
        private readonly Mock<IRequestClient<GetOrderRequest>> _orderClient = new();
        private readonly Mock<IRequestClient<GetPassengerRequest>> _passengerClient = new();
        private readonly Mock<ILogger<PassengerClient>> _logger = new();

        private PassengerClient Create() =>
            new PassengerClient(_orderClient.Object, _passengerClient.Object, _logger.Object);

        [Fact]
        public async Task GetOrderByPnrAndLastnameAsync_ReturnsOrder_WhenResponseSuccessful()
        {
            var expected = new OrderDto { OrderId = "PNR1" };
            var mockResp = new Mock<Response<GetOrderResponse>>();
            mockResp.Setup(r => r.Message).Returns(new GetOrderResponse { Order = expected });

            _orderClient
                .Setup(x => x.GetResponse<GetOrderResponse>(
                    It.IsAny<GetOrderRequest>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<RequestTimeout>()))
                .ReturnsAsync(mockResp.Object);

            var client = Create();
            var result = await client.GetOrderByPnrAndLastnameAsync("dyn", "PNR1", "Ivanov");

            Assert.NotNull(result);
            Assert.Equal("PNR1", result.OrderId);
        }

        [Fact]
        public async Task GetOrderByPnrAndLastnameAsync_Throws_WhenClientThrows()
        {
            _orderClient
                .Setup(x => x.GetResponse<GetOrderResponse>(
                    It.IsAny<GetOrderRequest>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<RequestTimeout>()))
                .ThrowsAsync(new InvalidOperationException());

            var client = Create();
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                client.GetOrderByPnrAndLastnameAsync("dyn", "PNR1", "Ivanov"));
        }

        [Fact]
        public async Task GetPassengerByIdAsync_ReturnsPassenger_WhenResponseSuccessful()
        {
            var expected = new PassengerDto { PassengerId = "P1" };
            var mockResp = new Mock<Response<GetPassengerResponse>>();
            mockResp.Setup(r => r.Message).Returns(new GetPassengerResponse { Passenger = expected });

            _passengerClient
                .Setup(x => x.GetResponse<GetPassengerResponse>(
                    It.IsAny<GetPassengerRequest>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<RequestTimeout>()))
                .ReturnsAsync(mockResp.Object);

            var client = Create();
            var result = await client.GetPassengerByIdAsync("dyn", "P1");

            Assert.NotNull(result);
            Assert.Equal("P1", result.PassengerId);
        }

        [Fact]
        public async Task GetPassengerByIdAsync_Throws_WhenClientThrows()
        {
            _passengerClient
                .Setup(x => x.GetResponse<GetPassengerResponse>(
                    It.IsAny<GetPassengerRequest>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<RequestTimeout>()))
                .ThrowsAsync(new Exception("fail"));

            var client = Create();
            await Assert.ThrowsAsync<Exception>(() =>
                client.GetPassengerByIdAsync("dyn", "P1"));
        }
    }
}