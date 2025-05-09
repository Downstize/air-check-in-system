using MassTransit;
using Shared.Contracts;
using Shared.Messages;

namespace Baggage.Clients;

public class PassengerClient
{
    private readonly IRequestClient<GetOrderRequest> _client;
    private readonly ILogger<PassengerClient> _logger;
    
    public PassengerClient(IRequestClient<GetOrderRequest> client, ILogger<PassengerClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<OrderDto> GetOrderAsync(string dynamicId, string orderId, string lastName)
    {
        _logger.LogInformation("Запрос к сервису пассажиров: DynamicId={DynamicId}, OrderId={OrderId}, LastName={LastName}",
            dynamicId, orderId, lastName);

        try
        {
            var response = await _client.GetResponse<GetOrderResponse>(new GetOrderRequest
            {
                DynamicId = dynamicId,
                OrderId = orderId,
                LastName = lastName
            });

            _logger.LogInformation("Успешно получен заказ: OrderId={OrderId}", response.Message.Order.OrderId);

            return response.Message.Order;
        }
        catch (RequestTimeoutException ex)
        {
            _logger.LogError(ex, "Таймаут при запросе заказа: DynamicId={DynamicId}, OrderId={OrderId}", dynamicId, orderId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при запросе заказа: DynamicId={DynamicId}, OrderId={OrderId}");
            throw;
        }
    }
}