using MassTransit;
using Microsoft.EntityFrameworkCore;
using SessionService.Data;
using Shared.Messages;

namespace SessionService.Consumers;

public class ValidateDynamicIdConsumer : IConsumer<ValidateDynamicIdRequest>
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ValidateDynamicIdConsumer> _logger;

    public ValidateDynamicIdConsumer(ApplicationDbContext db, ILogger<ValidateDynamicIdConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ValidateDynamicIdRequest> context)
    {
        var dynamicId = context.Message.DynamicId;
        _logger.LogInformation("Проверка валидности DynamicId: {DynamicId}", dynamicId);

        var exists = await _db.ActiveSessions
            .AnyAsync(s => s.DynamicId == dynamicId);

        _logger.LogInformation("Результат проверки DynamicId={DynamicId}: IsValid={IsValid}", dynamicId, exists);

        await context.RespondAsync(new ValidateDynamicIdResponse
        {
            IsValid = exists
        });
    }
}