namespace Shared.Messages;

public class GetPassengerBaggageWeightRequest
{
    public string DynamicId { get; set; }
    public string OrderId { get; set; }
    public string PassengerId { get; set; }
}