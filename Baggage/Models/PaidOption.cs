namespace Baggage.Models;

public class PaidOption
{
    public int    PaidOptionId { get; set; }
    public int    Pieces       { get; set; }
    public decimal WeightKg    { get; set; }
    public decimal Price       { get; set; }
}