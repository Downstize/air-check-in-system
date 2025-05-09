namespace Baggage.Models;

public class BaggageRegistration
{
    public string    RegistrationId { get; set; }
    public string DynamicId { get; set; } 
    public string OrderId        { get; set; }
    public string PassengerId    { get; set; }
    public int    Pieces         { get; set; }
    public decimal WeightKg      { get; set; }
    public decimal Price         { get; set; }
    public string TransactionId  { get; set; }
    public DateTime Timestamp    { get; set; }
}