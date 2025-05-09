using Shared.Contracts;

namespace Shared.Messages;

public class GetOrderResponse
{
    public OrderDto Order { get; set; }
}