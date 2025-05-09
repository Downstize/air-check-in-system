using MassTransit;
using Shared.Contracts;
using Shared.Messages;

namespace Registration.Clients;

public class PassengerClient
{
    private readonly IRequestClient<GetOrderRequest> _orderClient;
    private readonly IRequestClient<GetPassengerRequest> _passengerClient;

    public PassengerClient(
        IRequestClient<GetOrderRequest> orderClient,
        IRequestClient<GetPassengerRequest> passengerClient)
    {
        _orderClient = orderClient;
        _passengerClient = passengerClient;
    }

    public async Task<OrderDto?> GetOrderByPnrAndLastnameAsync(string dynamicId, string pnr, string lastName)
    {
        var response = await _orderClient.GetResponse<GetOrderResponse>(new GetOrderRequest
        {
            DynamicId = dynamicId,
            OrderId = pnr,
            LastName = lastName
        });

        return response.Message.Order;
    }

    public async Task<PassengerDto?> GetPassengerByIdAsync(string dynamicId, string passengerId)
    {
        var response = await _passengerClient.GetResponse<GetPassengerResponse>(new GetPassengerRequest
        {
            DynamicId = dynamicId,
            PassengerId = passengerId
        });

        return response.Message.Passenger;
    }

}