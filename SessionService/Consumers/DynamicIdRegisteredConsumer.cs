using MassTransit;
using Microsoft.EntityFrameworkCore;
using SessionService.Data;
using SessionService.Models;
using Shared.Messages;

namespace SessionService.Consumers;

public class DynamicIdRegisteredConsumer : IConsumer<DynamicIdRegistered>
{
    private readonly ApplicationDbContext _db;

    public DynamicIdRegisteredConsumer(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<DynamicIdRegistered> context)
    {
        var dynamicId = context.Message.DynamicId;

        if (!await _db.ActiveSessions.AnyAsync(s => s.DynamicId == dynamicId))
        {
            _db.ActiveSessions.Add(new ActiveSession
            {
                DynamicId = dynamicId,
                CreatedAt = context.Message.CreatedAt
            });

            await _db.SaveChangesAsync();

            Console.WriteLine($"DynamicId зарегистрирован: {dynamicId}");
        }
    }
}
