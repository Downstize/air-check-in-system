using Baggage.Models;

namespace Baggage.Data;

public static class DbInitializer
{
    public static void Seed(ApplicationDbContext db)
    {
        if (!db.PaidOptions.Any())
        {
            db.PaidOptions.AddRange(
                new PaidOption { PaidOptionId = Guid.NewGuid().ToString(), Pieces = 1, WeightKg = 20m, Price = 0m },
                new PaidOption { PaidOptionId = Guid.NewGuid().ToString(), Pieces = 2, WeightKg = 30m, Price = 50m },
                new PaidOption { PaidOptionId = Guid.NewGuid().ToString(), Pieces = 3, WeightKg = 40m, Price = 100m }
            );
        }

        if (!db.BaggagePayment.Any())
        {
            db.BaggagePayment.AddRange(
                new BaggagePayment
                {
                    PaymentId = Guid.NewGuid().ToString(), DynamicId = "DY001", PassengerId = "P001",
                    DepartureId = "D001", Amount = 0m, IsPaid = true, PaidAt = DateTime.UtcNow.AddDays(-3)
                },
                new BaggagePayment
                {
                    PaymentId = Guid.NewGuid().ToString(), DynamicId = "DY002", PassengerId = "P002",
                    DepartureId = "D002", Amount = 50m, IsPaid = true, PaidAt = DateTime.UtcNow.AddDays(-2)
                },
                new BaggagePayment
                {
                    PaymentId = Guid.NewGuid().ToString(), DynamicId = "DY003", PassengerId = "P003",
                    DepartureId = "D003", Amount = 100m, IsPaid = true, PaidAt = DateTime.UtcNow.AddDays(-1)
                },
                new BaggagePayment
                {
                    PaymentId = Guid.NewGuid().ToString(), DynamicId = "DY004", PassengerId = "P004",
                    DepartureId = "D004", Amount = 75m, IsPaid = false, PaidAt = DateTime.MinValue
                },
                new BaggagePayment
                {
                    PaymentId = Guid.NewGuid().ToString(), DynamicId = "DY005", PassengerId = "P005",
                    DepartureId = "D005", Amount = 20m, IsPaid = true, PaidAt = DateTime.UtcNow
                }
            );
        }

        if (!db.BaggageRegistrations.Any())
        {
            db.BaggageRegistrations.AddRange(
                new BaggageRegistration
                {
                    RegistrationId = Guid.NewGuid().ToString(), DynamicId = "DY001", OrderId = "O001",
                    PassengerId = "P001", Pieces = 1, WeightKg = 20m, Price = 0m, TransactionId = "TX001",
                    Timestamp = DateTime.UtcNow.AddDays(-3)
                },
                new BaggageRegistration
                {
                    RegistrationId = Guid.NewGuid().ToString(), DynamicId = "DY002", OrderId = "O002",
                    PassengerId = "P002", Pieces = 2, WeightKg = 30m, Price = 50m, TransactionId = "TX002",
                    Timestamp = DateTime.UtcNow.AddDays(-2)
                },
                new BaggageRegistration
                {
                    RegistrationId = Guid.NewGuid().ToString(), DynamicId = "DY003", OrderId = "O003",
                    PassengerId = "P003", Pieces = 3, WeightKg = 40m, Price = 100m, TransactionId = "TX003",
                    Timestamp = DateTime.UtcNow.AddDays(-1)
                },
                new BaggageRegistration
                {
                    RegistrationId = Guid.NewGuid().ToString(), DynamicId = "DY004", OrderId = "O004",
                    PassengerId = "P004", Pieces = 1, WeightKg = 10m, Price = 25m, TransactionId = "TX004",
                    Timestamp = DateTime.UtcNow
                },
                new BaggageRegistration
                {
                    RegistrationId = Guid.NewGuid().ToString(), DynamicId = "DY005", OrderId = "O005",
                    PassengerId = "P005", Pieces = 2, WeightKg = 25m, Price = 35m, TransactionId = "TX005",
                    Timestamp = DateTime.UtcNow
                }
            );
        }

        db.SaveChanges();
    }
}