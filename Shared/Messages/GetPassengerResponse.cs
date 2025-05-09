using Shared.Contracts;

namespace Shared.Messages;

public class GetPassengerResponse
{
    public PassengerDto Passenger { get; set; }
}