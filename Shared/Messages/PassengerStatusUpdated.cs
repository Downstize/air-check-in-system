namespace Shared.Messages;

public class PassengerStatusUpdated
{
    public string PassengerId { get; set; }
    public string NewStatus { get; set; }
}
