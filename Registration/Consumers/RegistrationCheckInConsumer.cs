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

    public RegistrationCheckInConsumer(
        IRegistrationService svc,
        IRequestClient<GetPassengerBaggageWeightRequest> baggageClient,
        IRequestClient<ValidateDynamicIdRequest> sessionClient,
        IPublishEndpoint publish,
        IOptions<RegistrationAuthOptions> authOptions)
    {
        _svc = svc;
        _baggageClient = baggageClient;
        _publish = publish;
        _sessionClient = sessionClient;
        _authOptions = authOptions.Value;
    }

    public async Task Consume(ConsumeContext<RegistrationCheckInRequest> context)
    {
        
        var msg = context.Message;
        
        var sessionResponse = await _sessionClient.GetResponse<ValidateDynamicIdResponse>(
            new ValidateDynamicIdRequest { DynamicId = msg.DynamicId });

        if (!sessionResponse.Message.IsValid)
            throw new Exception("Недействительный dynamicId");


        var auth = await _svc.AuthenticateAsync(new DTO.Auth.RegistrationAuthRequestDto
        {
            Login = _authOptions.Login,
            Pwd = _authOptions.Password
        });

        var dynamicId = msg.DynamicId;
        
        var orderResp = await _svc.SearchOrderAsync(new DTO.Order.RegistrationOrderSearchRequestDto
        {
            DynamicId = dynamicId,
            LastName = msg.LastName,
            PnrId = msg.Pnr
        });

        var seg = orderResp.Order.Segments.First();

        var pax = orderResp.Order.Passengers
            .FirstOrDefault(p => p.LastName.Equals(msg.LastName, StringComparison.OrdinalIgnoreCase));

        if (pax == null)
            throw new Exception($"Пассажир с фамилией {msg.LastName} не найден в заказе.");
        
        var seatResp = await _svc.ReserveSeatAsync(new DTO.Registration.RegistrationSeatReserveRequestDto
        {
            DynamicId = dynamicId,
            DepartureId = seg.DepartureId,
            PassengerId = pax.PassengerId,
            SeatNumber = pax.SeatNumber
        });

        DTO.Registration.RegistrationPassengerResponseDto regResp;

        if (msg.PaidSeat)
        {
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
        
        await _publish.Publish(new PassengerStatusUpdated
        {
            PassengerId = pax.PassengerId,
            NewStatus = pax.CheckInStatus
        });
        
        var baggageResponse = await _baggageClient.GetResponse<GetPassengerBaggageWeightResponse>(
            new GetPassengerBaggageWeightRequest
            {
                OrderId = orderResp.Order.OrderId,
                PassengerId = pax.PassengerId
            });

        var totalBaggageWeight = baggageResponse.Message.TotalWeightKg;
        
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
    }
}
