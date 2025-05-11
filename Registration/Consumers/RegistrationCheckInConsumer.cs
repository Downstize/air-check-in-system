using MassTransit;
using Microsoft.Extensions.Options;
using Registration.Models;
using Registration.Services;
using Shared.Messages;

namespace Registration.Consumers;

public class RegistrationCheckInConsumer : IConsumer<RegistrationCheckInRequest>
{
    private readonly IRegistrationService _svc;
    private readonly IRequestClient<GetPassengerBaggageWeightRequest> _baggageClient;
    private readonly IRequestClient<ValidateDynamicIdRequest> _sessionClient;
    private readonly IPublishEndpoint _publish;
    private readonly RegistrationAuthOptions _authOptions;
    private readonly ILogger<RegistrationCheckInConsumer> _logger;

    public RegistrationCheckInConsumer(
        IRegistrationService svc,
        IRequestClient<GetPassengerBaggageWeightRequest> baggageClient,
        IRequestClient<ValidateDynamicIdRequest> sessionClient,
        IPublishEndpoint publish,
        IOptions<RegistrationAuthOptions> authOptions,
        ILogger<RegistrationCheckInConsumer> logger)
    {
        _svc = svc;
        _baggageClient = baggageClient;
        _publish = publish;
        _sessionClient = sessionClient;
        _authOptions = authOptions.Value;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RegistrationCheckInRequest> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Получен запрос на регистрацию пассажира: PNR={Pnr}, LastName={LastName}", msg.Pnr, msg.LastName);

        var sessionResponse = await _sessionClient.GetResponse<ValidateDynamicIdResponse>(
            new ValidateDynamicIdRequest { DynamicId = msg.DynamicId });

        if (!sessionResponse.Message.IsValid)
        {
            _logger.LogWarning("Недействительный dynamicId: {DynamicId}", msg.DynamicId);
            throw new Exception("Недействительный dynamicId");
        }

        _logger.LogInformation("DynamicId {DynamicId} прошел проверку.", msg.DynamicId);

        var auth = await _svc.AuthenticateAsync(new DTO.Auth.RegistrationAuthRequestDto
        {
            Login = _authOptions.Login,
            Pwd = _authOptions.Password
        });

        _logger.LogInformation("Успешная аутентификация пользователя для регистрации.");

        var dynamicId = msg.DynamicId;

        var orderResp = await _svc.SearchOrderAsync(new DTO.Order.RegistrationOrderSearchRequestDto
        {
            DynamicId = dynamicId,
            LastName = msg.LastName,
            PnrId = msg.Pnr
        });

        _logger.LogInformation("Заказ найден: OrderId={OrderId}", orderResp.Order.OrderId);

        var seg = orderResp.Order.Segments.First();

        var pax = orderResp.Order.Passengers
            .FirstOrDefault(p => p.LastName.Equals(msg.LastName, StringComparison.OrdinalIgnoreCase));

        if (pax == null)
        {
            _logger.LogError("Пассажир с фамилией {LastName} не найден в заказе.", msg.LastName);
            throw new Exception($"Пассажир с фамилией {msg.LastName} не найден в заказе.");
        }

        _logger.LogInformation("Пассажир найден: PassengerId={PassengerId}", pax.PassengerId);

        var seatResp = await _svc.ReserveSeatAsync(new DTO.Registration.RegistrationSeatReserveRequestDto
        {
            DynamicId = dynamicId,
            DepartureId = seg.DepartureId,
            PassengerId = pax.PassengerId,
            SeatNumber = pax.SeatNumber
        });

        _logger.LogInformation("Место успешно зарезервировано: {SeatNumber}", seatResp.Seat.SeatNumber);

        DTO.Registration.RegistrationPassengerResponseDto regResp;

        if (msg.PaidSeat)
        {
            _logger.LogInformation("Платная регистрация для пассажира: {PassengerId}", pax.PassengerId);
            regResp = await _svc.RegisterPaidAsync(new DTO.Registration.RegistrationPassengerPaidRequestDto
            {
                DynamicId = dynamicId,
                DepartureId = seg.DepartureId,
                PassengerId = pax.PassengerId,
                SeatNumber = seatResp.Seat.SeatNumber
            });
        }
        else
        {
            _logger.LogInformation("Бесплатная регистрация для пассажира: {PassengerId}", pax.PassengerId);
            regResp = await _svc.RegisterFreeAsync(new DTO.Registration.RegistrationPassengerFreeRequestDto
            {
                DynamicId = dynamicId,
                DepartureId = seg.DepartureId,
                PassengerId = pax.PassengerId,
                SeatNumber = seatResp.Seat.SeatNumber
            });
        }

        pax.CheckInStatus = regResp.IsPaid ? "agent_checked" : "web_checked";
        pax.SeatNumber = regResp.SeatNumber;

        _logger.LogInformation("Регистрация завершена: CheckInStatus={Status}, Seat={Seat}", pax.CheckInStatus, pax.SeatNumber);

        await _publish.Publish(new PassengerStatusUpdated
        {
            PassengerId = pax.PassengerId,
            NewStatus = pax.CheckInStatus
        });

        _logger.LogInformation("Событие PassengerStatusUpdated опубликовано: PassengerId={PassengerId}, Status={Status}", pax.PassengerId, pax.CheckInStatus);

        var baggageResponse = await _baggageClient.GetResponse<GetPassengerBaggageWeightResponse>(
            new GetPassengerBaggageWeightRequest
            {
                OrderId = orderResp.Order.OrderId,
                PassengerId = pax.PassengerId
            });

        var totalBaggageWeight = baggageResponse.Message.TotalWeightKg;

        _logger.LogInformation("Общий вес багажа для пассажира: {Weight} кг", totalBaggageWeight);

        await context.RespondAsync(new RegistrationCheckInResponse
        {
            Order = new Shared.Contracts.OrderDto
            {
                OrderId = orderResp.Order.OrderId,
                Segments = orderResp.Order.Segments,
                Passengers = new List<Shared.Contracts.PassengerDto> { pax },
                LuggageWeight = totalBaggageWeight > 0 ? totalBaggageWeight : orderResp.Order.LuggageWeight,
                PaidCheckin = orderResp.Order.PaidCheckin
            }
        });

        _logger.LogInformation("Ответ успешно отправлен по регистрации заказа: {OrderId}", orderResp.Order.OrderId);
    }
}
