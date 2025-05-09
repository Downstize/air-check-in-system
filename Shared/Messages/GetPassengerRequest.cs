namespace Shared.Messages;

public class GetPassengerRequest
{
    public string DynamicId { get; set; }
    public string PassengerId { get; set; }
}