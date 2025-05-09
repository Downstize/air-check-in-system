using Baggage.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Messages;

namespace Baggage.Consumers;

public class GetPassengerBaggageWeightConsumer : IConsumer<GetPassengerBaggageWeightRequest>
{
    private readonly ApplicationDbContext _db;
    private readonly IRequestClient<ValidateDynamicIdRequest> _sessionClient;
    private readonly ILogger<GetPassengerBaggageWeightConsumer> _logger;

    public GetPassengerBaggageWeightConsumer(
        ApplicationDbContext db,
        IRequestClient<ValidateDynamicIdRequest> sessionClient,
        ILogger<GetPassengerBaggageWeightConsumer> logger)
    {
        _db = db;
        _sessionClient = sessionClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GetPassengerBaggageWeightRequest> context)
    {
        var req = context.Message;
        
        _logger.LogInformation(
            "Получен запрос на получение веса багажа: DynamicId={DynamicId}, OrderId={OrderId}, PassengerId={PassengerId}",
            req.DynamicId, req.OrderId, req.PassengerId);

        try
        {
            var sessionResponse = await _sessionClient.GetResponse<ValidateDynamicIdResponse>(
                new ValidateDynamicIdRequest { DynamicId = req.DynamicId });

            if (!sessionResponse.Message.IsValid)
            {
                _logger.LogWarning(
                    "DynamicId невалидный: {DynamicId}", req.DynamicId);

                await context.RespondAsync(new GetPassengerBaggageWeightResponse
                {
                    TotalWeightKg = 0
                });
                return;
            }

            var totalWeight = await _db.BaggageRegistrations
                .Where(r => r.OrderId == req.OrderId && r.PassengerId == req.PassengerId)
                .SumAsync(r => (decimal?)r.WeightKg) ?? 0;

            _logger.LogInformation(
                "Общий вес багажа найден: {TotalWeightKg} кг для PassengerId={PassengerId}", totalWeight, req.PassengerId);

            await context.RespondAsync(new GetPassengerBaggageWeightResponse
            {
                TotalWeightKg = totalWeight
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Ошибка при обработке запроса веса багажа: DynamicId={DynamicId}, OrderId={OrderId}, PassengerId={PassengerId}",
                req.DynamicId, req.OrderId, req.PassengerId);

            throw;
        }
    }
}
