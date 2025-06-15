using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Registration.Clients;
using Registration.Data;
using Registration.Models;
using Registration.Services;
using Registration.DTO.Registration;
using Shared.Messages;
using MassTransit;

namespace Registration.Tests.Services
{
    public class RegistrationServiceReservationTests
    {
        private RegistrationService CreateService(ApplicationDbContext db)
        {
            var oc = new Mock<IRequestClient<GetOrderRequest>>().Object;
            var pc = new Mock<IRequestClient<GetPassengerRequest>>().Object;
            var passengerClient = new PassengerClient(
                oc,
                pc,
                new Mock<ILogger<PassengerClient>>().Object);

            return new RegistrationService(
                db,
                passengerClient,
                new Mock<IPublishEndpoint>().Object,
                new Mock<ILogger<RegistrationService>>().Object,
                new Mock<IDistributedCache>().Object);
        }

        [Fact]
        public async Task ReserveSeatAsync_Throws_WhenSeatTakenByOther()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var db = new ApplicationDbContext(options);

            db.SeatReservations.Add(new SeatReservation
            {
                DynamicId = "d",
                DepartureId = "D",
                PassengerId = "X",
                SeatNumber = "1A"
            });
            await db.SaveChangesAsync();

            var svc = CreateService(db);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.ReserveSeatAsync(new RegistrationSeatReserveRequestDto
                {
                    DynamicId = "d",
                    DepartureId = "D",
                    PassengerId = "Y",
                    SeatNumber = "1A"
                }));
        }

        [Fact]
        public async Task ReserveSeatAsync_ReturnsExisting_WhenSamePassenger()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var db = new ApplicationDbContext(options);

            db.SeatReservations.Add(new SeatReservation
            {
                DynamicId = "d",
                DepartureId = "D",
                PassengerId = "P",
                SeatNumber = "1A"
            });
            await db.SaveChangesAsync();

            var svc = CreateService(db);
            var result = await svc.ReserveSeatAsync(new RegistrationSeatReserveRequestDto
            {
                DynamicId = "d",
                DepartureId = "D",
                PassengerId = "P",
                SeatNumber = "1A"
            });
            Assert.Equal("1A", result.Seat.SeatNumber);
        }
    }
}