using SessionService.Services;
using Microsoft.AspNetCore.Mvc;

namespace SessionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionsService _svc;
    private readonly ILogger<SessionController> _logger;

    public SessionController(ISessionsService svc, ILogger<SessionController> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string dynamicId)
    {
        _logger.LogInformation("Регистрация сессии: DynamicId={DynamicId}", dynamicId);
        await _svc.RegisterSessionAsync(dynamicId);
        _logger.LogInformation("Сессия успешно зарегистрирована: DynamicId={DynamicId}", dynamicId);
        return Ok();
    }

    [HttpGet("validate")]
    public async Task<IActionResult> Validate(string dynamicId)
    {
        _logger.LogInformation("Проверка сессии: DynamicId={DynamicId}", dynamicId);
        var valid = await _svc.ValidateSessionAsync(dynamicId);
        _logger.LogInformation("Результат проверки: DynamicId={DynamicId}, IsValid={IsValid}", dynamicId, valid);
        return valid ? Ok() : Unauthorized();
    }
}