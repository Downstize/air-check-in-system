using SessionService.Models;

namespace SessionService.Services;

public interface ISessionsService
{
    Task<bool> ValidateSessionAsync(string dynamicId);
    Task RegisterSessionAsync(string dynamicId);
    
    Task<IEnumerable<ActiveSession>> GetAllAsync();
    Task<ActiveSession?> GetByIdAsync(int id);
    Task<ActiveSession> CreateAsync(ActiveSession session);
    Task<ActiveSession?> UpdateAsync(int id, ActiveSession session);
    Task<bool> DeleteAsync(int id);
}