namespace Shared.Contracts;

public class BaggageRegistrationDto
{
    public string DynamicId { get; set; } = default!;
    public string OrderId { get; set; } = default!;
    public string PassengerId { get; set; } = default!;
    public int Pieces { get; set; }
    public decimal WeightKg { get; set; }
    public decimal Price { get; set; }
}