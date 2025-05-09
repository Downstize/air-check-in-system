using Microsoft.EntityFrameworkCore;
using PassengerService.Data;
using Shared.Messages;
using MassTransit;
using Shared.Contracts;

namespace PassengerService.Consumers;

public class GetPassengerConsumer : IConsumer<GetPassengerRequest>
{
    private readonly ApplicationDbContext _db;

    public GetPassengerConsumer(ApplicationDbContext db) => _db = db;

    public async Task Consume(ConsumeContext<GetPassengerRequest> context)
    {
        var req = context.Message;

        var p = await _db.Passengers
            .Include(p => p.Document)
            .Include(p => p.VisaDocument)
            .FirstOrDefaultAsync(p => p.PassengerId == req.PassengerId);

        if (p == null)
            throw new KeyNotFoundException($"Passenger {req.PassengerId} не найден.");

        var dto = new PassengerDto
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
        };

        await context.RespondAsync(new GetPassengerResponse
        {
            Passenger = dto
        });
    }
}
