namespace Baggage.DTO;

public class BaggageAllowanceDto
{
    public string DynamicId   { get; set; }
    public string OrderId     { get; set; }
    public string PassengerId { get; set; }
    public int    FreePieces  { get; set; }
    public decimal FreeWeightKg { get; set; }
    public List<PaidOptionDto> PaidOptions { get; set; }
}