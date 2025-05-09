namespace Registration.Models;

public class Payment
{
    public int PaymentId { get; set; }
    public string DynamicId { get; set; }
    public string PassengerId { get; set; }
    public string DepartureId { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime PaidAt { get; set; }
}