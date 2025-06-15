using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Registration.Clients;
using Registration.Data;
using Registration.Services;
using Shared.Messages;
using Microsoft.EntityFrameworkCore;

namespace Registration.Tests.Services
{
    public class RegistrationServiceRegisterTests
    {
        private RegistrationService CreateService(
            ApplicationDbContext db,
            Mock<IRequestClient<GetOrderRequest>> orderClient,
            Mock<IRequestClient<GetPassengerRequest>> passengerClient)
        {
            var pc = new PassengerClient(
                orderClient.Object,
                passengerClient.Object,
                new Mock<ILogger<PassengerClient>>().Object);

            return new RegistrationService(
                db,
                pc,
                new Mock<IPublishEndpoint>().Object,
                new Mock<ILogger<RegistrationService>>().Object,
                new Mock<IDistributedCache>().Object);
        }

        [Fact]
        public async Task RegisterFreeAsync_Throws_WhenPassengerNotFound()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var db = new ApplicationDbContext(options);

            var oc = new Mock<IRequestClient<GetOrderRequest>>();
            var pc = new Mock<IRequestClient<GetPassengerRequest>>();

            var mockResp = new Mock<Response<GetPassengerResponse>>();
            mockResp.Setup(r => r.Message).Returns(new GetPassengerResponse { Passenger = null });
            pc.Setup(x => x.GetResponse<GetPassengerResponse>(
                    It.IsAny<GetPassengerRequest>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<RequestTimeout>()))
                .ReturnsAsync(mockResp.Object);

            var svc = CreateService(db, oc, pc);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                svc.RegisterFreeAsync(new Registration.DTO.Registration.RegistrationPassengerFreeRequestDto()));
        }
    }
}