using Microsoft.EntityFrameworkCore;
using PassengerService.Data;
using PassengerService.Models;

namespace PassengerService.Tests.Utilities
{
    public static class TestDbContextFactory
    {
        public static ApplicationDbContext CreateInMemory()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var ctx = new ApplicationDbContext(options);

            ctx.Flights.Add(new Flight
            {
                FlightId = 1,
                FlightNumber = "SU1001",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                AircompanyCode = "SU",
                DeparturePortCode = "SVO",
                ArrivalPortCode = "JFK",
                FlightStatus = "scheduled"
            });
            ctx.Bookings.Add(new Booking
            {
                BookingId = 1,
                Pnr = "AB1234",
                FlightId = 1,
                LuggageWeight = 20m,
                PaidCheckin = false
            });
            ctx.SaveChanges();

            static DateTime Utc(int y, int m, int d)
            {
                var dt = new DateTime(y, m, d, 0, 0, 0);
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }

            ctx.Passengers.Add(new Passenger
            {
                PassengerId = "EXISTING",
                BookingId = 1,
                PnrId = "AB1234",
                PaxNo = 1,
                LastName = "Ivanov",
                FirstName = "Ivan",
                BirthDate = Utc(1990, 1, 1),
                Category = "ADT",
                CheckInStatus = "not_checked",
                SeatsOccupied = 1,
                Eticket = true,
                SeatNumber = "10A",
                SeatStatus = "available",
                SeatLayerType = "economy",
                Remarks = new(),
                Apis = 0,
                Document = new PassengerDocument
                {
                    Type = "passport",
                    IssueCountryCode = "RU",
                    Number = "P1234561",
                    NationalityCode = "RU",
                    BirthDate = Utc(1990, 1, 1),
                    ExpiryDate = Utc(2030, 1, 1)
                },
                VisaDocument = new PassengerVisaDocument
                {
                    BirthPlace = "Moscow",
                    Number = "VISA001",
                    IssuePlace = "Moscow Consulate",
                    IssueDate = Utc(2020, 1, 1),
                    ApplicCountryCode = "US"
                }
            });
            ctx.SaveChanges();

            return ctx;
        }
    }
}