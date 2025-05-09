namespace Baggage.DTO.Requests;

public class CancelRequest {
    public string DynamicId   { get; set; }
    public string OrderId     { get; set; }
    public string PassengerId { get; set; }
}