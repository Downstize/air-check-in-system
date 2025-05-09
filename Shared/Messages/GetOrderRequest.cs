namespace Shared.Messages;

public class GetOrderRequest
{
    public string DynamicId { get; set; }
    public string OrderId { get; set; }
    public string LastName { get; set; }
}