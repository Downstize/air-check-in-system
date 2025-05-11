using MassTransit;
using Microsoft.EntityFrameworkCore;
using SessionService.Data;
using SessionService.Models;
using Shared.Messages;

namespace SessionService.Consumers;

public class DynamicIdRegisteredConsumer : IConsumer<DynamicIdRegistered>
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DynamicIdRegisteredConsumer> _logger;

    public DynamicIdRegisteredConsumer(ApplicationDbContext db, ILogger<DynamicIdRegisteredConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DynamicIdRegistered> context)
    {
        var dynamicId = context.Message.DynamicId;
        _logger.LogInformation("Получено сообщение о регистрации DynamicId: {DynamicId}", dynamicId);

        if (!await _db.ActiveSessions.AnyAsync(s => s.DynamicId == dynamicId))
        {
            _db.ActiveSessions.Add(new ActiveSession
            {
                DynamicId = dynamicId,
                CreatedAt = context.Message.CreatedAt
            });

            await _db.SaveChangesAsync();

            _logger.LogInformation("DynamicId успешно зарегистрирован и сохранён в БД: {DynamicId}", dynamicId);
        }
        else
        {
            _logger.LogWarning("DynamicId уже существует: {DynamicId}", dynamicId);
        }
    }
}