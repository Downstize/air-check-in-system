using MassTransit;
using Shared.Contracts;
using Shared.Messages;

namespace Registration.Clients;

public class PassengerClient
{
    private readonly IRequestClient<GetOrderRequest> _orderClient;
    private readonly IRequestClient<GetPassengerRequest> _passengerClient;
    private readonly ILogger<PassengerClient> _logger;

    public PassengerClient(
        IRequestClient<GetOrderRequest> orderClient,
        IRequestClient<GetPassengerRequest> passengerClient,
        ILogger<PassengerClient> logger)
    {
        _orderClient = orderClient;
        _passengerClient = passengerClient;
        _logger = logger;
    }

    public async Task<OrderDto?> GetOrderByPnrAndLastnameAsync(string dynamicId, string pnr, string lastName)
    {
        _logger.LogInformation("Запрос заказа по PNR: {Pnr} и фамилии: {LastName}", pnr, lastName);
        try
        {
            var response = await _orderClient.GetResponse<GetOrderResponse>(new GetOrderRequest
            {
                DynamicId = dynamicId,
                OrderId = pnr,
                LastName = lastName
            });

            _logger.LogInformation("Успешно получен заказ по PNR: {Pnr}", pnr);
            return response.Message.Order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении заказа по PNR: {Pnr} и фамилии: {LastName}", pnr, lastName);
            throw;
        }
    }

    public async Task<PassengerDto?> GetPassengerByIdAsync(string dynamicId, string passengerId)
    {
        _logger.LogInformation("Запрос пассажира по ID: {PassengerId}", passengerId);
        try
        {
            var response = await _passengerClient.GetResponse<GetPassengerResponse>(new GetPassengerRequest
            {
                DynamicId = dynamicId,
                PassengerId = passengerId
            });

            _logger.LogInformation("Успешно получен пассажир с ID: {PassengerId}", passengerId);
            return response.Message.Passenger;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении пассажира с ID: {PassengerId}", passengerId);
            throw;
        }
    }
}
