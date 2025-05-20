using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SessionService.Models;
using SessionService.Services;

namespace SessionService.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/admin/session")]
public class AdminSessionController : ControllerBase
{
    private readonly ISessionsService _service;
    private readonly ILogger<AdminSessionController> _logger;

    public AdminSessionController(ISessionsService service, ILogger<AdminSessionController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("ADMIN: Запрос списка всех сессий");
        var result = await _service.GetAllAsync();
        _logger.LogInformation("ADMIN: Получено {Count} сессий", result.Count());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("ADMIN: Запрос сессии по ID — {Id}", id);
        var session = await _service.GetByIdAsync(id);
        if (session == null)
        {
            _logger.LogWarning("ADMIN: Сессия не найдена — ID={Id}", id);
            return NotFound();
        }
        _logger.LogInformation("ADMIN: Сессия найдена — ID={Id}", id);
        return Ok(session);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ActiveSession session)
    {
        _logger.LogInformation("ADMIN: Создание сессии — DynamicId={DynamicId}", session.DynamicId);
        var result = await _service.CreateAsync(session);
        _logger.LogInformation("ADMIN: Сессия создана — ID={Id}", result.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActiveSession session)
    {
        _logger.LogInformation("ADMIN: Обновление сессии — ID={Id}", id);
        var result = await _service.UpdateAsync(id, session);
        if (result == null)
        {
            _logger.LogWarning("ADMIN: Сессия не найдена для обновления — ID={Id}", id);
            return NotFound();
        }
        _logger.LogInformation("ADMIN: Сессия обновлена — ID={Id}", id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("ADMIN: Удаление сессии — ID={Id}", id);
        var success = await _service.DeleteAsync(id);
        if (!success)
        {
            _logger.LogWarning("ADMIN: Сессия не найдена для удаления — ID={Id}", id);
            return NotFound();
        }
        _logger.LogInformation("ADMIN: Сессия удалена — ID={Id}", id);
        return NoContent();
    }
}
