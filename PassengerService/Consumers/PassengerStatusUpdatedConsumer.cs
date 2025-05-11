namespace PassengerService.Consumers;

using MassTransit;
using PassengerService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Messages;

public class PassengerStatusUpdatedConsumer : IConsumer<PassengerStatusUpdated>
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<PassengerStatusUpdatedConsumer> _logger;

    public PassengerStatusUpdatedConsumer(ApplicationDbContext db, ILogger<PassengerStatusUpdatedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PassengerStatusUpdated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Получено сообщение об обновлении статуса пассажира: ID = {PassengerId}, Новый статус = {NewStatus}", msg.PassengerId, msg.NewStatus);

        var passenger = await _db.Passengers.FirstOrDefaultAsync(p => p.PassengerId == msg.PassengerId);
        if (passenger == null)
        {
            _logger.LogWarning("Пассажир с ID {PassengerId} не найден в базе данных", msg.PassengerId);
            return;
        }

        passenger.CheckInStatus = msg.NewStatus;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Статус пассажира с ID {PassengerId} успешно обновлён на {NewStatus}", msg.PassengerId, msg.NewStatus);
    }
}