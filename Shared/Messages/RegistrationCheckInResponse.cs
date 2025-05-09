using Shared.Contracts;

namespace Shared.Messages;

public class RegistrationCheckInResponse
{
    public OrderDto Order { get; set; } = default!;
}