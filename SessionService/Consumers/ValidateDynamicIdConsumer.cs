using MassTransit;
using Microsoft.EntityFrameworkCore;
using SessionService.Data;
using Shared.Messages;

namespace SessionService.Consumers;

public class ValidateDynamicIdConsumer : IConsumer<ValidateDynamicIdRequest>
{
    private readonly ApplicationDbContext _db;

    public ValidateDynamicIdConsumer(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<ValidateDynamicIdRequest> context)
    {
        Console.WriteLine($"Проверка! Пришёл DynamicId: {context.Message.DynamicId}");

        var exists = await _db.ActiveSessions
            .AnyAsync(s => s.DynamicId == context.Message.DynamicId);

        await context.RespondAsync(new ValidateDynamicIdResponse
        {
            IsValid = exists
        });
    }
}