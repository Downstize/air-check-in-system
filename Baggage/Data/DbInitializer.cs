using Baggage.Models;

namespace Baggage.Data;

public static class DbInitializer
{
    public static void Seed(ApplicationDbContext db)
    {
        if (!db.PaidOptions.Any())
        {
            db.PaidOptions.Add(new PaidOption
            {
                Pieces = 1,
                WeightKg = 20m,
                Price = 0m
            });

            db.PaidOptions.Add(new PaidOption
            {
                Pieces = 2,
                WeightKg = 30m,
                Price = 50m
            });

            db.PaidOptions.Add(new PaidOption
            {
                Pieces = 3,
                WeightKg = 40m,
                Price = 100m
            });

            db.SaveChanges();
        }
    }
}
