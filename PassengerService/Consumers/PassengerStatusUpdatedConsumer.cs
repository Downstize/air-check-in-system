namespace PassengerService.Consumers;

using MassTransit;
using PassengerService.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Messages;

public class PassengerStatusUpdatedConsumer : IConsumer<PassengerStatusUpdated>
{
    private readonly ApplicationDbContext _db;

    public PassengerStatusUpdatedConsumer(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<PassengerStatusUpdated> context)
    {
        var msg = context.Message;

        var passenger = await _db.Passengers.FirstOrDefaultAsync(p => p.PassengerId == msg.PassengerId);
        if (passenger == null)
            return;

        passenger.CheckInStatus = msg.NewStatus;

        await _db.SaveChangesAsync();
    }
}