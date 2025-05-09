namespace Shared.Messages;

public class BaggageRegistrationRequest
{
    public string DynamicId { get; set; } = default!;
    public string Pnr { get; set; } = default!;
    public string PassengerId { get; set; } = default!;
    public int    Pieces { get; set; }
    public decimal Weight { get; set; }
}