using PassengerService.Data;
using Shared.Contracts;
using Shared.Messages;

namespace PassengerService.Consumers;

using MassTransit;
using Microsoft.EntityFrameworkCore;


public class GetOrderConsumer : IConsumer<GetOrderRequest>
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<GetOrderConsumer> _logger;

    public GetOrderConsumer(ApplicationDbContext db, ILogger<GetOrderConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GetOrderRequest> context)
    {
        var req = context.Message;
        
        _logger.LogInformation("Получен запрос на получение заказа: OrderId={OrderId}, DynamicId={DynamicId}, LastName={LastName}",
            req.OrderId, req.DynamicId, req.LastName);

        try
        {
            var booking = await _db.Bookings
                .Include(b => b.Passengers).ThenInclude(p => p.Document)
                .Include(b => b.Passengers).ThenInclude(p => p.VisaDocument)
                .Include(b => b.Flight)
                .FirstOrDefaultAsync(b => b.Pnr == req.OrderId);

            if (booking == null)
            {
                _logger.LogWarning("Booking не найден по PNR: {OrderId}", req.OrderId);
                await context.RespondAsync(new GetOrderResponse
                {
                    Order = null!
                });
                return;
            }

            var order = new OrderDto
            {
                OrderId = booking.Pnr,
                LuggageWeight = booking.LuggageWeight,
                PaidCheckin = booking.PaidCheckin,
                Segments = new List<FlightSegmentDto>
                {
                    new FlightSegmentDto
                    {
                        DepartureId = booking.Flight.FlightId.ToString(),
                        AircompanyCode = booking.Flight.AircompanyCode,
                        FlightNumber = booking.Flight.FlightNumber,
                        DepartureTime = booking.Flight.DepartureTime,
                        ArrivalTime = booking.Flight.ArrivalTime,
                        DeparturePortCode = booking.Flight.DeparturePortCode,
                        ArrivalPortCode = booking.Flight.ArrivalPortCode,
                        FlightStatus = booking.Flight.FlightStatus
                    }
                },
                Passengers = booking.Passengers.Select(p => new PassengerDto
                {
                    PassengerId = p.PassengerId,
                    PnrId = p.PnrId,
                    PaxNo = p.PaxNo,
                    LastName = p.LastName,
                    FirstName = p.FirstName,
                    BirthDate = p.BirthDate,
                    Category = p.Category,
                    CheckInStatus = p.CheckInStatus,
                    Reason = p.Reason,
                    SeatsOccupied = p.SeatsOccupied,
                    Eticket = p.Eticket,
                    Document = new PassengerDocumentDto
                    {
                        Type = p.Document.Type,
                        IssueCountryCode = p.Document.IssueCountryCode,
                        Number = p.Document.Number,
                        NationalityCode = p.Document.NationalityCode,
                        BirthDate = p.Document.BirthDate,
                        ExpiryDate = p.Document.ExpiryDate
                    },
                    VisaDocument = new PassengerVisaDocumentDto
                    {
                        BirthPlace = p.VisaDocument.BirthPlace,
                        Number = p.VisaDocument.Number,
                        IssuePlace = p.VisaDocument.IssuePlace,
                        IssueDate = p.VisaDocument.IssueDate,
                        ApplicCountryCode = p.VisaDocument.ApplicCountryCode
                    },
                    SeatNumber = p.SeatNumber,
                    SeatStatus = p.SeatStatus,
                    SeatLayerType = p.SeatLayerType,
                    Remarks = p.Remarks ?? new(),
                    Apis = p.Apis,
                    BookingId = p.BookingId
                }).ToList()
            };

            _logger.LogInformation("Успешно найден booking и сформирован OrderDto для OrderId={OrderId}", req.OrderId);

            await context.RespondAsync(new GetOrderResponse
            {
                Order = order
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке запроса GetOrder: OrderId={OrderId}", req.OrderId);
            throw;
        }
    }
}
