namespace Baggage.DTO.Requests;

public class RegisterRequest {
    public string DynamicId   { get; set; }
    public string OrderId     { get; set; }
    public string PassengerId { get; set; }
    public int    Pieces      { get; set; }
    public decimal WeightKg   { get; set; }
}