namespace Baggage.Models;

public class PaidOption
{
    public string PaidOptionId { get; set; } = Guid.NewGuid().ToString();
    public int Pieces { get; set; }
    public decimal WeightKg { get; set; }
    public decimal Price { get; set; }
}