using SessionService.Data;
using SessionService.Models;
using Microsoft.EntityFrameworkCore;

namespace SessionService.Services;

public class SessionService : ISessionService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SessionService> _logger;

    public SessionService(ApplicationDbContext db, ILogger<SessionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> ValidateSessionAsync(string dynamicId)
    {
        _logger.LogInformation("Проверка сессии по DynamicId: {DynamicId}", dynamicId);
        var exists = await _db.ActiveSessions.AnyAsync(s => s.DynamicId == dynamicId);
        _logger.LogInformation("Результат проверки сессии: DynamicId={DynamicId}, Exists={Exists}", dynamicId, exists);
        return exists;
    }

    public async Task RegisterSessionAsync(string dynamicId)
    {
        _logger.LogInformation("Регистрация новой сессии: DynamicId={DynamicId}", dynamicId);

        if (!await _db.ActiveSessions.AnyAsync(s => s.DynamicId == dynamicId))
        {
            _db.ActiveSessions.Add(new ActiveSession { DynamicId = dynamicId });
            await _db.SaveChangesAsync();
            _logger.LogInformation("Сессия успешно зарегистрирована: DynamicId={DynamicId}", dynamicId);
        }
        else
        {
            _logger.LogWarning("Сессия с таким DynamicId уже существует: {DynamicId}", dynamicId);
        }
    }
}