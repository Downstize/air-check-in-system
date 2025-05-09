using SessionService.Services;
using Microsoft.AspNetCore.Mvc;

namespace SessionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _svc;

    public SessionController(ISessionService svc) => _svc = svc;

    [HttpPost("register")]
    public async Task<IActionResult> Register(string dynamicId)
    {
        await _svc.RegisterSessionAsync(dynamicId);
        return Ok();
    }

    [HttpGet("validate")]
    public async Task<IActionResult> Validate(string dynamicId)
    {
        var valid = await _svc.ValidateSessionAsync(dynamicId);
        return valid ? Ok() : Unauthorized();
    }
}