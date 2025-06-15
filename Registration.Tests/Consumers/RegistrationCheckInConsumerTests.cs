using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Registration.Consumers;
using Registration.Services;
using Shared.Messages;
using Microsoft.Extensions.Options;
using Registration.Models;
using Shared.Contracts;

namespace Registration.Tests.Consumers
{
    public class RegistrationCheckInConsumerTests
    {
        private readonly Mock<IRegistrationService> _svc = new();
        private readonly Mock<IRequestClient<GetPassengerBaggageWeightRequest>> _baggage = new();
        private readonly Mock<IRequestClient<ValidateDynamicIdRequest>> _session = new();
        private readonly Mock<IPublishEndpoint> _publish = new();
        private readonly Mock<ILogger<RegistrationCheckInConsumer>> _logger = new();
        private readonly RegistrationAuthOptions _authOptions = new() { Login = "u", Password = "p" };

        private RegistrationCheckInConsumer Create()
        {
            var opts = Mock.Of<IOptions<RegistrationAuthOptions>>(o => o.Value == _authOptions);
            return new RegistrationCheckInConsumer(
                _svc.Object, _baggage.Object, _session.Object, _publish.Object, opts, _logger.Object);
        }

        [Fact]
        public async Task Consume_PublishesPassengerStatusUpdated_AndResponds()
        {
            var msg = new RegistrationCheckInRequest
            {
                DynamicId = "d",
                Pnr = "PNR",
                LastName = "Ivanov",
                PaidSeat = false
            };
            var ctx = new Mock<ConsumeContext<RegistrationCheckInRequest>>();
            ctx.SetupGet(c => c.Message).Returns(msg);

            ctx.Setup(c => c.RespondAsync<RegistrationCheckInResponse>(
                    It.IsAny<RegistrationCheckInResponse>()))
                .Returns(Task.CompletedTask);

            var validSession = new Mock<Response<ValidateDynamicIdResponse>>();
            validSession.Setup(r => r.Message).Returns(new ValidateDynamicIdResponse { IsValid = true });
            _session.Setup(c => c.GetResponse<ValidateDynamicIdResponse>(
                    It.IsAny<ValidateDynamicIdRequest>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<RequestTimeout>()))
                .ReturnsAsync(validSession.Object);

            var orderDto = new OrderDto
            {
                OrderId = "PNR",
                Segments = new List<FlightSegmentDto> { new() { DepartureId = "D1" } },
                Passengers = new List<PassengerDto>
                    { new() { PassengerId = "P1", LastName = "Ivanov", SeatNumber = "1A" } },
                LuggageWeight = 0m,
                PaidCheckin = false
            };
            _svc.Setup(x => x.AuthenticateAsync(It.IsAny<Registration.DTO.Auth.RegistrationAuthRequestDto>()))
                .ReturnsAsync(new Registration.DTO.Auth.RegistrationAuthResponseDto { DynamicId = "d" });
            _svc.Setup(x => x.SearchOrderAsync(It.IsAny<Registration.DTO.Order.RegistrationOrderSearchRequestDto>()))
                .ReturnsAsync(new Registration.DTO.Order.RegistrationOrderSearchResponseDto { Order = orderDto });
            _svc.Setup(x =>
                    x.ReserveSeatAsync(It.IsAny<Registration.DTO.Registration.RegistrationSeatReserveRequestDto>()))
                .ReturnsAsync(new Registration.DTO.Registration.RegistrationSeatReserveResponseDto
                    { Seat = new Registration.DTO.Shared.SeatDto { SeatNumber = "1A" } });
            _svc.Setup(x =>
                    x.RegisterFreeAsync(It.IsAny<Registration.DTO.Registration.RegistrationPassengerFreeRequestDto>()))
                .ReturnsAsync(new Registration.DTO.Registration.RegistrationPassengerResponseDto
                    { IsPaid = false, SeatNumber = "1A" });

            var baggageResp = new Mock<Response<GetPassengerBaggageWeightResponse>>();
            baggageResp.Setup(r => r.Message).Returns(new GetPassengerBaggageWeightResponse { TotalWeightKg = 5 });
            _baggage.Setup(c => c.GetResponse<GetPassengerBaggageWeightResponse>(
                    It.IsAny<GetPassengerBaggageWeightRequest>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<RequestTimeout>()))
                .ReturnsAsync(baggageResp.Object);

            var consumer = Create();
            await consumer.Consume(ctx.Object);

            _publish.Verify(x => x.Publish(
                It.Is<PassengerStatusUpdated>(m => m.PassengerId == "P1" && m.NewStatus == "web_checked"),
                It.IsAny<CancellationToken>()), Times.Once);

            ctx.Verify(c => c.RespondAsync<RegistrationCheckInResponse>(
                    It.Is<RegistrationCheckInResponse>(r => r.Order.OrderId == "PNR")),
                Times.Once);
        }
    }
}