using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Prometheus;
using Shared.Contracts;
using Shared.Messages;
using AirCheckInOrchestrator.Api.Controllers;
using AirCheckInOrchestrator.Api.DTO.Requests;

namespace AirCheckInOrchestrator.Api.Tests.Controllers
{
    public class OrchestratorControllerTests
    {
        private readonly Mock<IRequestClient<RegistrationCheckInRequest>> _regClient;
        private readonly Mock<IRequestClient<BaggageRegistrationRequest>> _bagClient;
        private readonly Mock<ILogger<OrchestratorController>> _loggerMock;
        private readonly Counter _checkInCounter;
        private readonly Counter _baggageCounter;
        private readonly OrchestratorController _controller;

        public OrchestratorControllerTests()
        {
            _regClient = new Mock<IRequestClient<RegistrationCheckInRequest>>();
            _bagClient = new Mock<IRequestClient<BaggageRegistrationRequest>>();
            _loggerMock = new Mock<ILogger<OrchestratorController>>();
            _checkInCounter = Metrics.CreateCounter("test_checkin", "Test checkin counter");
            _baggageCounter = Metrics.CreateCounter("test_baggage", "Test baggage counter");

            _controller = new OrchestratorController(
                _bagClient.Object,
                _regClient.Object,
                _loggerMock.Object,
                _checkInCounter,
                _baggageCounter);
        }

        [Fact]
        public async Task CheckIn_Success_ReturnsOrderDto()
        {
            var request = new CheckInRequest
            {
                DynamicId = "D1",
                LastName = "Ivanov",
                Pnr = "PNR1",
                PaidSeat = false
            };
            var responseMessage = new RegistrationCheckInResponse
            {
                Order = new OrderDto { OrderId = "PNR1" }
            };
            var responseMock = Task.FromResult(
                Mock.Of<Response<RegistrationCheckInResponse>>(r =>
                    r.Message == responseMessage));

            _regClient
                .Setup(c => c.GetResponse<RegistrationCheckInResponse>(
                    It.IsAny<RegistrationCheckInRequest>(),
                    default,
                    default))
                .Returns(responseMock);

            var result = await _controller.CheckIn(request);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(responseMessage.Order, ok.Value);
        }

        [Fact]
        public async Task CheckIn_Failure_ReturnsServerError()
        {
            _regClient
                .Setup(c => c.GetResponse<RegistrationCheckInResponse>(
                    It.IsAny<RegistrationCheckInRequest>(),
                    default,
                    default))
                .ThrowsAsync(new Exception("fail"));

            var result = await _controller.CheckIn(new CheckInRequest());

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task AddBaggage_Success_ReturnsRegistrationDto()
        {
            var request = new BaggageRequest
            {
                DynamicId = "D1",
                Pnr = "PNR1",
                PassengerId = "P1",
                Pieces = 1,
                Weight = 10m
            };
            var responseMessage = new BaggageRegistrationResponse
            {
                Registration = new BaggageRegistrationDto { DynamicId = "D1" }
            };
            var responseMock = Task.FromResult(
                Mock.Of<Response<BaggageRegistrationResponse>>(r =>
                    r.Message == responseMessage));

            _bagClient
                .Setup(c => c.GetResponse<BaggageRegistrationResponse>(
                    It.IsAny<BaggageRegistrationRequest>(),
                    default,
                    default))
                .Returns(responseMock);

            var result = await _controller.AddBaggage(request);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(responseMessage.Registration, ok.Value);
        }

        [Fact]
        public async Task AddBaggage_Failure_ReturnsServerError()
        {
            _bagClient
                .Setup(c => c.GetResponse<BaggageRegistrationResponse>(
                    It.IsAny<BaggageRegistrationRequest>(),
                    default,
                    default))
                .ThrowsAsync(new Exception("fail"));

            var result = await _controller.AddBaggage(new BaggageRequest());

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }
    }
}