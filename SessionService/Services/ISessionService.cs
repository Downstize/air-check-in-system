namespace SessionService.Services;

public interface ISessionService
{
    Task<bool> ValidateSessionAsync(string dynamicId);
    Task RegisterSessionAsync(string dynamicId);
}