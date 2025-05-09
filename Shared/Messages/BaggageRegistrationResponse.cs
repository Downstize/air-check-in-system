using Shared.Contracts;

namespace Shared.Messages;

public class BaggageRegistrationResponse
{
    public BaggageRegistrationDto Registration { get; set; } = default!;
}