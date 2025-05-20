namespace Baggage.Models;

public class BaggagePayment
{
    public string PaymentId { get; set; }
    public string DynamicId { get; set; }
    public string PassengerId { get; set; }
    public string DepartureId { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime PaidAt { get; set; }
}