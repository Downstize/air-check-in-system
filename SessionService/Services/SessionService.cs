using SessionService.Data;
using SessionService.Models;
using Microsoft.EntityFrameworkCore;

namespace SessionService.Services;

public class SessionService : ISessionService
{
    private readonly ApplicationDbContext _db;

    public SessionService(ApplicationDbContext db) => _db = db;

    public async Task<bool> ValidateSessionAsync(string dynamicId)
    {
        return await _db.ActiveSessions.AnyAsync(s => s.DynamicId == dynamicId);
    }

    public async Task RegisterSessionAsync(string dynamicId)
    {
        if (!await _db.ActiveSessions.AnyAsync(s => s.DynamicId == dynamicId))
        {
            _db.ActiveSessions.Add(new ActiveSession { DynamicId = dynamicId });
            await _db.SaveChangesAsync();
        }
    }
}