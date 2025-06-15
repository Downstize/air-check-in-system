using PassengerService.Models;

namespace PassengerService.Data;

public static class DbInitializer
{
    public static void Seed(ApplicationDbContext db)
    {
        if (db.Flights.Any()) return;

        var f1 = new Flight
        {
            FlightNumber = "SU1001",
            DepartureTime = DateTime.UtcNow.AddDays(1),
            ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            DeparturePortCode = "SVO",
            ArrivalPortCode = "JFK",
            AircompanyCode = "SU",
            FlightStatus = "scheduled"
        };
        var f2 = new Flight
        {
            FlightNumber = "SU2002",
            DepartureTime = DateTime.UtcNow.AddDays(2),
            ArrivalTime = DateTime.UtcNow.AddDays(2).AddHours(3),
            DeparturePortCode = "VKO",
            ArrivalPortCode = "LHR",
            AircompanyCode = "SU",
            FlightStatus = "scheduled"
        };
        var f3 = new Flight
        {
            FlightNumber = "SU3003",
            DepartureTime = DateTime.UtcNow.AddDays(3),
            ArrivalTime = DateTime.UtcNow.AddDays(3).AddHours(4),
            DeparturePortCode = "DME",
            ArrivalPortCode = "CDG",
            AircompanyCode = "SU",
            FlightStatus = "scheduled"
        };


        var b1 = new Booking { Pnr = "AB1234", Flight = f1 };
        var b2 = new Booking { Pnr = "CD5678", Flight = f2 };
        var b3 = new Booking { Pnr = "EF9012", Flight = f3 };

        var passengers = new List<Passenger>
        {
            new Passenger
            {
                PassengerId = Guid.NewGuid().ToString(),
                PnrId = "AB1234", PaxNo = 1, LastName = "Ivanov", FirstName = "Ivan",
                BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Category = "ADT", CheckInStatus = "not_checked", SeatsOccupied = 1,
                Eticket = true, SeatNumber = "10A", SeatStatus = "available",
                SeatLayerType = "economy", Remarks = new List<string>(), Apis = 0,
                Document = new PassengerDocument
                {
                    Type = "passport", IssueCountryCode = "RU", Number = "P1234561",
                    NationalityCode = "RU", BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ExpiryDate = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                VisaDocument = new PassengerVisaDocument
                {
                    BirthPlace = "Moscow", Number = "VISA001", IssuePlace = "Moscow Consulate",
                    IssueDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), ApplicCountryCode = "US"
                },
                Booking = b1
            },
            new Passenger
            {
                PassengerId = Guid.NewGuid().ToString(),
                PnrId = "AB1234", PaxNo = 2, LastName = "Petrova", FirstName = "Maria",
                BirthDate = new DateTime(1991, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                Category = "ADT", CheckInStatus = "not_checked", SeatsOccupied = 1,
                Eticket = true, SeatNumber = "10B", SeatStatus = "available",
                SeatLayerType = "economy", Remarks = new List<string>(), Apis = 0,
                Document = new PassengerDocument
                {
                    Type = "passport", IssueCountryCode = "RU", Number = "P1234562",
                    NationalityCode = "RU", BirthDate = new DateTime(1991, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                    ExpiryDate = new DateTime(2031, 2, 2, 0, 0, 0, DateTimeKind.Utc)
                },
                VisaDocument = new PassengerVisaDocument
                {
                    BirthPlace = "Moscow", Number = "VISA002", IssuePlace = "Moscow Consulate",
                    IssueDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), ApplicCountryCode = "US"
                },
                Booking = b1
            },
            new Passenger
            {
                PassengerId = Guid.NewGuid().ToString(),
                PnrId = "AB1234", PaxNo = 3, LastName = "Sidorov", FirstName = "Oleg",
                BirthDate = new DateTime(1992, 3, 3, 0, 0, 0, DateTimeKind.Utc),
                Category = "ADT", CheckInStatus = "not_checked", SeatsOccupied = 1,
                Eticket = true, SeatNumber = "10C", SeatStatus = "available",
                SeatLayerType = "economy", Remarks = new List<string>(), Apis = 0,
                Document = new PassengerDocument
                {
                    Type = "passport", IssueCountryCode = "RU", Number = "P1234563",
                    NationalityCode = "RU", BirthDate = new DateTime(1992, 3, 3, 0, 0, 0, DateTimeKind.Utc),
                    ExpiryDate = new DateTime(2032, 3, 3, 0, 0, 0, DateTimeKind.Utc)
                },
                VisaDocument = new PassengerVisaDocument
                {
                    BirthPlace = "Moscow", Number = "VISA003", IssuePlace = "Moscow Consulate",
                    IssueDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), ApplicCountryCode = "US"
                },
                Booking = b1
            },
            new Passenger
            {
                PassengerId = Guid.NewGuid().ToString(),
                PnrId = "AB1234", PaxNo = 4, LastName = "Volkova", FirstName = "Anna",
                BirthDate = new DateTime(1993, 4, 4, 0, 0, 0, DateTimeKind.Utc),
                Category = "ADT", CheckInStatus = "not_checked", SeatsOccupied = 1,
                Eticket = true, SeatNumber = "11A", SeatStatus = "available",
                SeatLayerType = "economy", Remarks = new List<string>(), Apis = 0,
                Document = new PassengerDocument
                {
                    Type = "passport", IssueCountryCode = "RU", Number = "P1234564",
                    NationalityCode = "RU", BirthDate = new DateTime(1993, 4, 4, 0, 0, 0, DateTimeKind.Utc),
                    ExpiryDate = new DateTime(2033, 4, 4, 0, 0, 0, DateTimeKind.Utc)
                },
                VisaDocument = new PassengerVisaDocument
                {
                    BirthPlace = "Moscow", Number = "VISA004", IssuePlace = "Moscow Consulate",
                    IssueDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), ApplicCountryCode = "US"
                },
                Booking = b1
            },
            new Passenger
            {
                PassengerId = Guid.NewGuid().ToString(),
                PnrId = "AB1234", PaxNo = 5, LastName = "Smirnov", FirstName = "Sergey",
                BirthDate = new DateTime(1994, 5, 5, 0, 0, 0, DateTimeKind.Utc),
                Category = "ADT", CheckInStatus = "not_checked", SeatsOccupied = 1,
                Eticket = true, SeatNumber = "11B", SeatStatus = "available",
                SeatLayerType = "economy", Remarks = new List<string>(), Apis = 0,
                Document = new PassengerDocument
                {
                    Type = "passport", IssueCountryCode = "RU", Number = "P1234565",
                    NationalityCode = "RU", BirthDate = new DateTime(1994, 5, 5, 0, 0, 0, DateTimeKind.Utc),
                    ExpiryDate = new DateTime(2034, 5, 5, 0, 0, 0, DateTimeKind.Utc)
                },
                VisaDocument = new PassengerVisaDocument
                {
                    BirthPlace = "Moscow", Number = "VISA005", IssuePlace = "Moscow Consulate",
                    IssueDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), ApplicCountryCode = "US"
                },
                Booking = b1
            },

            new Passenger
            {
                PassengerId = Guid.NewGuid().ToString(), PnrId = "CD5678", PaxNo = 1, LastName = "Kuznetsov",
                FirstName = "Pavel", BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc), Category = "ADT",
                CheckInStatus = "not_checked", SeatsOccupied = 1, Eticket = true, SeatNumber = "12A",
                SeatStatus = "available", SeatLayerType = "economy", Remarks = new List<string>(), Apis = 0,
                Document = new PassengerDocument
                {
                    Type = "passport", IssueCountryCode = "RU", Number = "P2234561", NationalityCode = "RU",
                    BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ExpiryDate = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                VisaDocument = new PassengerVisaDocument
                {
                    BirthPlace = "Moscow", Number = "VISA101", IssuePlace = "Moscow Consulate",
                    IssueDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), ApplicCountryCode = "US"
                },
                Booking = b2
            },
            new Passenger
            {
                PassengerId = Guid.NewGuid().ToString(), PnrId = "CD5678", PaxNo = 2, LastName = "Alexeeva",
                FirstName = "Elena", BirthDate = new DateTime(1991, 2, 2, 0, 0, 0, DateTimeKind.Utc), Category = "ADT",
                CheckInStatus = "not_checked", SeatsOccupied = 1, Eticket = true, SeatNumber = "12B",
                SeatStatus = "available", SeatLayerType = "economy", Remarks = new List<string>(), Apis = 0,
                Document = new PassengerDocument
                {
                    Type = "passport", IssueCountryCode = "RU", Number = "P2234562", NationalityCode = "RU",
                    BirthDate = new DateTime(1991, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                    ExpiryDate = new DateTime(2031, 2, 2, 0, 0, 0, DateTimeKind.Utc)
                },
                VisaDocument = new PassengerVisaDocument
                {
                    BirthPlace = "Moscow", Number = "VISA102", IssuePlace = "Moscow Consulate",
                    IssueDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), ApplicCountryCode = "US"
                },
                Booking = b2
            },
            new Passenger
            {
                PassengerId = Guid.NewGuid().ToString(), PnrId = "CD5678", PaxNo = 3, LastName = "Fedorov",
                FirstName = "Dmitry", BirthDate = new DateTime(1992, 3, 3, 0, 0, 0, DateTimeKind.Utc), Category = "ADT",
                CheckInStatus = "not_checked", SeatsOccupied = 1, Eticket = true, SeatNumber = "12C",
                SeatStatus = "available", SeatLayerType = "economy", Remarks = new List<string>(), Apis = 0,
                Document = new PassengerDocument
                {
                    Type = "passport", IssueCountryCode = "RU", Number = "P2234563", NationalityCode = "RU",
                    BirthDate = new DateTime(1992, 3, 3, 0, 0, 0, DateTimeKind.Utc),
                    ExpiryDate = new DateTime(2032, 3, 3, 0, 0, 0, DateTimeKind.Utc)
                },
                VisaDocument = new PassengerVisaDocument
                {
                    BirthPlace = "Moscow", Number = "VISA103", IssuePlace = "Moscow Consulate",
                    IssueDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), ApplicCountryCode = "US"
                },
                Booking = b2
            },
            new Passenger
            {
                PassengerId = Guid.NewGuid().ToString(), PnrId = "CD5678", PaxNo = 4, LastName = "Morozova",
                FirstName = "Olga", BirthDate = new DateTime(1993, 4, 4, 0, 0, 0, DateTimeKind.Utc), Category = "ADT",
                CheckInStatus = "not_checked", SeatsOccupied = 1, Eticket = true, SeatNumber = "13A",
                SeatStatus = "available", SeatLayerType = "economy", Remarks = new List<string>(), Apis = 0,
                Document = new PassengerDocument
                {
                    Type = "passport", IssueCountryCode = "RU", Number = "P2234564", NationalityCode = "RU",
                    BirthDate = new DateTime(1993, 4, 4, 0, 0, 0, DateTimeKind.Utc),
                    ExpiryDate = new DateTime(2033, 4, 4, 0, 0, 0, DateTimeKind.Utc)
                },
                VisaDocument = new PassengerVisaDocument
                {
                    BirthPlace = "Moscow", Number = "VISA104", IssuePlace = "Moscow Consulate",
                    IssueDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), ApplicCountryCode = "US"
                },
                Booking = b2
            },
            new Passenger
            {
                PassengerId = Guid.NewGuid().ToString(), PnrId = "CD5678", PaxNo = 5, LastName = "Vasiliev",
                FirstName = "Igor", BirthDate = new DateTime(1994, 5, 5, 0, 0, 0, DateTimeKind.Utc), Category = "ADT",
                CheckInStatus = "not_checked", SeatsOccupied = 1, Eticket = true, SeatNumber = "13B",
                SeatStatus = "available", SeatLayerType = "economy", Remarks = new List<string>(), Apis = 0,
                Document = new PassengerDocument
                {
                    Type = "passport", IssueCountryCode = "RU", Number = "P2234565", NationalityCode = "RU",
                    BirthDate = new DateTime(1994, 5, 5, 0, 0, 0, DateTimeKind.Utc),
                    ExpiryDate = new DateTime(2034, 5, 5, 0, 0, 0, DateTimeKind.Utc)
                },
                VisaDocument = new PassengerVisaDocument
                {
                    BirthPlace = "Moscow", Number = "VISA105", IssuePlace = "Moscow Consulate",
                    IssueDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), ApplicCountryCode = "US"
                },
                Booking = b2
            },
        };

        db.Flights.AddRange(f1, f2, f3);
        db.Bookings.AddRange(b1, b2, b3);
        db.Passengers.AddRange(passengers);

        db.SaveChanges();
    }
}