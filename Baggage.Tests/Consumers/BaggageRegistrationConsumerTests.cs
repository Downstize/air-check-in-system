using Baggage.Consumers;
using Baggage.DTO;
using Baggage.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Contracts;
using Shared.Messages;

namespace Baggage.Tests.Consumers
{
    public class BaggageRegistrationConsumerTests
    {
        private static (BaggageRegistrationConsumer consumer,
            Mock<IBaggageService> svcMock,
            Mock<ConsumeContext<BaggageRegistrationRequest>> ctxMock)
            Arrange(bool dynamicIdIsValid)
        {
            var request = new BaggageRegistrationRequest
            {
                DynamicId = "D1",
                Pnr = "PNR1",
                PassengerId = "P1",
                Pieces = 1,
                Weight = 10
            };

            var ctxMock = new Mock<ConsumeContext<BaggageRegistrationRequest>>();
            ctxMock.SetupGet(c => c.Message).Returns(request);

            var validateResp = new Mock<Response<ValidateDynamicIdResponse>>();
            validateResp.SetupGet(r => r.Message)
                .Returns(new ValidateDynamicIdResponse { IsValid = dynamicIdIsValid });

            var sessionClientMock = new Mock<IRequestClient<ValidateDynamicIdRequest>>();
            sessionClientMock
                .Setup(c => c.GetResponse<ValidateDynamicIdResponse>(
                    It.IsAny<ValidateDynamicIdRequest>(), default, default))
                .ReturnsAsync(validateResp.Object);

            var svcMock = new Mock<IBaggageService>();
            svcMock.Setup(s => s.GetAllowanceAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new BaggageAllowanceDto { DynamicId = "D1" });

            svcMock.Setup(s => s.RegisterAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1, 10))
                .ReturnsAsync(new BaggageRegistrationDto { PassengerId = "P1" });

            var logger = new Mock<ILogger<BaggageRegistrationConsumer>>();
            var consumer = new BaggageRegistrationConsumer(svcMock.Object, sessionClientMock.Object, logger.Object);

            return (consumer, svcMock, ctxMock);
        }

        [Fact]
        public async Task Consume_InvalidDynamicId_Responds_With_Null()
        {
            var (consumer, svcMock, ctxMock) = Arrange(dynamicIdIsValid: false);

            await consumer.Consume(ctxMock.Object);

            ctxMock.Verify(c => c.RespondAsync(
                    It.Is<BaggageRegistrationResponse>(r => r.Registration == null)),
                Times.Once);
            svcMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Consume_Valid_Request_Calls_Service_And_Responds()
        {
            var (consumer, svcMock, ctxMock) = Arrange(dynamicIdIsValid: true);

            await consumer.Consume(ctxMock.Object);

            svcMock.Verify(s => s.RegisterAsync(
                It.IsAny<string>(), "PNR1", "P1", 1, 10m), Times.Once);

            ctxMock.Verify(c => c.RespondAsync(
                    It.Is<BaggageRegistrationResponse>(r => r.Registration != null)),
                Times.Once);
        }
    }
}