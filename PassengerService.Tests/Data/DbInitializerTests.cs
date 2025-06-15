using Microsoft.EntityFrameworkCore;
using PassengerService.Data;

namespace PassengerService.Tests.Data
{
    public class DbInitializerTests
    {
        [Fact]
        public void Seed_PopulatesFlightsBookingsPassengers()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("SeedTest")
                .Options;
            using var db = new ApplicationDbContext(options);

            DbInitializer.Seed(db);

            Assert.Equal(3, db.Flights.Count());
            Assert.Equal(3, db.Bookings.Count());
            Assert.True(db.Passengers.Count() > 5);
        }
    }
}