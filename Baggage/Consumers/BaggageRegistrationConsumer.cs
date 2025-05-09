using Baggage.Services;
using Shared.Messages;

namespace Baggage.Consumers;

using MassTransit;


public class BaggageRegistrationConsumer : IConsumer<BaggageRegistrationRequest>
{
    private readonly IBaggageService _svc;
    private readonly IRequestClient<ValidateDynamicIdRequest> _sessionClient;
    private readonly ILogger<BaggageRegistrationConsumer> _logger;

    public BaggageRegistrationConsumer(
        IBaggageService svc,
        IRequestClient<ValidateDynamicIdRequest> sessionClient,
        ILogger<BaggageRegistrationConsumer> logger)
    {
        _svc = svc;
        _sessionClient = sessionClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BaggageRegistrationRequest> context)
    {
        var msg = context.Message;
        
        _logger.LogInformation(
            "Получен запрос на регистрацию багажа: DynamicId={DynamicId}, PNR={Pnr}, PassengerId={PassengerId}, Pieces={Pieces}, Weight={Weight}",
            msg.DynamicId, msg.Pnr, msg.PassengerId, msg.Pieces, msg.Weight);
        
        try
        {
            var sessionResponse = await _sessionClient.GetResponse<ValidateDynamicIdResponse>(
                new ValidateDynamicIdRequest { DynamicId = msg.DynamicId });

            if (!sessionResponse.Message.IsValid)
            {
                _logger.LogWarning(
                    "DynamicId невалидный: {DynamicId}", msg.DynamicId);

                await context.RespondAsync(new BaggageRegistrationResponse
                {
                    Registration = null
                });
                return;
            }

            var allowance = await _svc.GetAllowanceAsync(
                dynamicId: msg.DynamicId,
                orderId: msg.Pnr,
                passengerId: msg.PassengerId);

            _logger.LogInformation(
                "Получено разрешение на багаж для PassengerId={PassengerId}", msg.PassengerId);

            var reg = await _svc.RegisterAsync(
                dynamicId: allowance.DynamicId,
                orderId: msg.Pnr,
                passengerId: msg.PassengerId,
                pieces: msg.Pieces,
                weightKg: msg.Weight);

            _logger.LogInformation(
                "Багаж успешно зарегистрирован для: PassengerId={PassengerId}", reg.PassengerId);

            await context.RespondAsync(new BaggageRegistrationResponse
            {
                Registration = reg
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Ошибка при регистрации багажа для PassengerId={PassengerId}, PNR={Pnr}", msg.PassengerId, msg.Pnr);

            throw;
        }
    }
}
