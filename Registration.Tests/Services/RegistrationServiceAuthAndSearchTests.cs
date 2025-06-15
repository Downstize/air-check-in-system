using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Registration.Clients;
using Registration.Data;
using Registration.Services;
using Shared.Messages;

namespace Registration.Tests.Services
{
    public class RegistrationServiceAuthAndSearchTests
    {
        private RegistrationService CreateService(
            out ApplicationDbContext db,
            out Mock<IRequestClient<GetOrderRequest>> orderClient,
            out Mock<IRequestClient<GetPassengerRequest>> passengerClient,
            out Mock<IPublishEndpoint> pubMock)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            db = new ApplicationDbContext(options);

            orderClient = new Mock<IRequestClient<GetOrderRequest>>();
            passengerClient = new Mock<IRequestClient<GetPassengerRequest>>();
            pubMock = new Mock<IPublishEndpoint>();

            var pc = new PassengerClient(
                orderClient.Object,
                passengerClient.Object,
                new Mock<ILogger<PassengerClient>>().Object);

            return new RegistrationService(
                db,
                pc,
                pubMock.Object,
                new Mock<ILogger<RegistrationService>>().Object,
                new Mock<IDistributedCache>().Object);
        }

        [Fact]
        public async Task AuthenticateAsync_PublishesAndReturnsDynamicId()
        {
            var svc = CreateService(out var db, out var oc, out var pc, out var pub);
            var resp = await svc.AuthenticateAsync(new DTO.Auth.RegistrationAuthRequestDto());
            Assert.False(string.IsNullOrEmpty(resp.DynamicId));
            pub.Verify(x => x.Publish(It.IsAny<DynamicIdRegistered>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SearchOrderAsync_Throws_WhenOrderNull()
        {
            var svc = CreateService(out var db, out var oc, out var pc, out var pub);

            var mockResp = new Mock<Response<GetOrderResponse>>();
            mockResp.Setup(r => r.Message).Returns(new GetOrderResponse { Order = null });
            oc.Setup(x => x.GetResponse<GetOrderResponse>(
                    It.IsAny<GetOrderRequest>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<RequestTimeout>()))
                .ReturnsAsync(mockResp.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                svc.SearchOrderAsync(new DTO.Order.RegistrationOrderSearchRequestDto
                {
                    DynamicId = "d", PnrId = "PNR1", LastName = "Ivanov"
                }));
        }
    }
}