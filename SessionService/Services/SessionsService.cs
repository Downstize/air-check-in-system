using System.Text.Json;
using SessionService.Data;
using SessionService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace SessionService.Services;

public class SessionsService : ISessionsService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SessionsService> _logger;
    private readonly IDistributedCache _cache;

    public SessionsService(ApplicationDbContext db, ILogger<SessionsService> logger, IDistributedCache cache)
    {
        _db = db;
        _logger = logger;
        _cache = cache;
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

    public async Task<IEnumerable<ActiveSession>> GetAllAsync()
    {
        const string cacheKey = "admin:sessions";
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("ADMIN (SessionService): Получены сессии из кэша");
            return JsonSerializer.Deserialize<IEnumerable<ActiveSession>>(cached)!;
        }
        
        await Task.Delay(300);

        var sessions = await _db.ActiveSessions.ToListAsync();
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(sessions),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        return sessions;
    }

    public async Task<ActiveSession?> GetByIdAsync(int id)
    {
        var cached = await _cache.GetStringAsync($"admin:sessions:{id}");
        if (cached != null)
        {
            _logger.LogInformation("ADMIN (SessionService): Сессия {Id} получена из кэша", id);
            return JsonSerializer.Deserialize<ActiveSession>(cached);
        }
        
        await Task.Delay(300);

        _logger.LogInformation("ADMIN (SessionService): Запрос сессии {Id} из БД", id);
        var session = await _db.ActiveSessions.FirstOrDefaultAsync(s => s.Id == id);

        if (session != null)
        {
            await _cache.SetStringAsync($"admin:sessions:{id}", JsonSerializer.Serialize(session),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

            _logger.LogInformation("ADMIN (SessionService): Сессия {Id} закэширована", id);
        }

        return session;
    }

    public async Task<ActiveSession> CreateAsync(ActiveSession session)
    {
        _logger.LogInformation("ADMIN (SessionService): Создание новой сессии с DynamicId {DynamicId}",
            session.DynamicId);
        _db.ActiveSessions.Add(session);
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync("admin:sessions");
        return session;
    }

    public async Task<ActiveSession?> UpdateAsync(int id, ActiveSession session)
    {
        var entity = await _db.ActiveSessions.FindAsync(id);
        if (entity == null) return null;

        entity.DynamicId = session.DynamicId;
        await _db.SaveChangesAsync();

        await _cache.RemoveAsync("admin:sessions");
        await _cache.RemoveAsync($"admin:sessions:{id}");
        _logger.LogInformation("ADMIN (SessionService): Обновлена сессия с ID {SessionId}", id);

        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.ActiveSessions.FindAsync(id);
        if (entity == null) return false;

        _db.ActiveSessions.Remove(entity);
        await _db.SaveChangesAsync();

        await _cache.RemoveAsync("admin:sessions");
        await _cache.RemoveAsync($"admin:sessions:{id}");
        _logger.LogWarning("ADMIN (SessionService): Удалена сессия с ID {SessionId}", id);
        return true;
    }
}