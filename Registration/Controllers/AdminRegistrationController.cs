using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Registration.Models;
using Registration.Services;

namespace Registration.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/admin/registration")]
public class AdminRegistrationController : ControllerBase
{
    private readonly IRegistrationService _service;
    private readonly ILogger<AdminRegistrationController> _logger;

    public AdminRegistrationController(IRegistrationService service, ILogger<AdminRegistrationController> logger)
    {
        _service = service;
        _logger = logger;
    }
    
    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments()
    {
        _logger.LogInformation("ADMIN: Запрос списка всех платежей");
        var result = await _service.GetAllPaymentsAsync();
        _logger.LogInformation("ADMIN: Получено {Count} платежей", result.Count());
        return Ok(result);
    }

    [HttpGet("payments/{id}")]
    public async Task<IActionResult> GetPayment(int id)
    {
        _logger.LogInformation("ADMIN: Запрос платежа по ID — {Id}", id);
        var payment = await _service.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            _logger.LogWarning("ADMIN: Платёж не найден — ID={Id}", id);
            return NotFound();
        }
        _logger.LogInformation("ADMIN: Платёж найден — ID={Id}", id);
        return Ok(payment);
    }

    [HttpPost("payments")]
    public async Task<IActionResult> CreatePayment([FromBody] Payment payment)
    {
        _logger.LogInformation("ADMIN: Создание платежа — PassengerId={PassengerId}", payment.PassengerId);
        var result = await _service.CreatePaymentAsync(payment);
        _logger.LogInformation("ADMIN: Платёж создан — ID={Id}", result.PaymentId);
        return CreatedAtAction(nameof(GetPayment), new { id = result.PaymentId }, result);
    }

    [HttpPut("payments/{id}")]
    public async Task<IActionResult> UpdatePayment(int id, [FromBody] Payment payment)
    {
        _logger.LogInformation("ADMIN: Обновление платежа — ID={Id}", id);
        var result = await _service.UpdatePaymentAsync(id, payment);
        if (result == null)
        {
            _logger.LogWarning("ADMIN: Платёж не найден для обновления — ID={Id}", id);
            return NotFound();
        }
        _logger.LogInformation("ADMIN: Платёж обновлён — ID={Id}", id);
        return NoContent();
    }

    [HttpDelete("payments/{id}")]
    public async Task<IActionResult> DeletePayment(int id)
    {
        _logger.LogInformation("ADMIN: Удаление платежа — ID={Id}", id);
        var success = await _service.DeletePaymentAsync(id);
        if (!success)
        {
            _logger.LogWarning("ADMIN: Платёж не найден для удаления — ID={Id}", id);
            return NotFound();
        }
        _logger.LogInformation("ADMIN: Платёж удалён — ID={Id}", id);
        return NoContent();
    }
    
    [HttpGet("registrations")]
    public async Task<IActionResult> GetRegistrations()
    {
        _logger.LogInformation("ADMIN: Запрос списка всех регистраций");
        var result = await _service.GetAllRegistrationsAsync();
        _logger.LogInformation("ADMIN: Получено {Count} регистраций", result.Count());
        return Ok(result);
    }

    [HttpGet("registrations/{id}")]
    public async Task<IActionResult> GetRegistration(int id)
    {
        _logger.LogInformation("ADMIN: Запрос регистрации по ID — {Id}", id);
        var record = await _service.GetRegistrationByIdAsync(id);
        if (record == null)
        {
            _logger.LogWarning("ADMIN: Регистрация не найдена — ID={Id}", id);
            return NotFound();
        }
        _logger.LogInformation("ADMIN: Регистрация найдена — ID={Id}", id);
        return Ok(record);
    }

    [HttpPost("registrations")]
    public async Task<IActionResult> CreateRegistration([FromBody] RegistrationRecord record)
    {
        _logger.LogInformation("ADMIN: Создание регистрации — PassengerId={PassengerId}", record.PassengerId);
        var result = await _service.CreateRegistrationAsync(record);
        _logger.LogInformation("ADMIN: Регистрация создана — ID={Id}", result.RegistrationRecordId);
        return CreatedAtAction(nameof(GetRegistration), new { id = result.RegistrationRecordId }, result);
    }

    [HttpPut("registrations/{id}")]
    public async Task<IActionResult> UpdateRegistration(int id, [FromBody] RegistrationRecord record)
    {
        _logger.LogInformation("ADMIN: Обновление регистрации — ID={Id}", id);
        var result = await _service.UpdateRegistrationAsync(id, record);
        if (result == null)
        {
            _logger.LogWarning("ADMIN: Регистрация не найдена для обновления — ID={Id}", id);
            return NotFound();
        }
        _logger.LogInformation("ADMIN: Регистрация обновлена — ID={Id}", id);
        return NoContent();
    }

    [HttpDelete("registrations/{id}")]
    public async Task<IActionResult> DeleteRegistration(int id)
    {
        _logger.LogInformation("ADMIN: Удаление регистрации — ID={Id}", id);
        var success = await _service.DeleteRegistrationAsync(id);
        if (!success)
        {
            _logger.LogWarning("ADMIN: Регистрация не найдена для удаления — ID={Id}", id);
            return NotFound();
        }
        _logger.LogInformation("ADMIN: Регистрация удалена — ID={Id}", id);
        return NoContent();
    }
    
    [HttpGet("reservations")]
    public async Task<IActionResult> GetReservations()
    {
        _logger.LogInformation("ADMIN: Запрос списка всех резерваций");
        var result = await _service.GetAllReservationsAsync();
        _logger.LogInformation("ADMIN: Получено {Count} резерваций", result.Count());
        return Ok(result);
    }

    [HttpGet("reservations/{id}")]
    public async Task<IActionResult> GetReservation(int id)
    {
        _logger.LogInformation("ADMIN: Запрос резервации по ID — {Id}", id);
        var reservation = await _service.GetReservationByIdAsync(id);
        if (reservation == null)
        {
            _logger.LogWarning("ADMIN: Резервация не найдена — ID={Id}", id);
            return NotFound();
        }
        _logger.LogInformation("ADMIN: Резервация найдена — ID={Id}", id);
        return Ok(reservation);
    }

    [HttpPost("reservations")]
    public async Task<IActionResult> CreateReservation([FromBody] SeatReservation reservation)
    {
        _logger.LogInformation("ADMIN: Создание резервации — PassengerId={PassengerId}, SeatNumber={SeatNumber}", reservation.PassengerId, reservation.SeatNumber);
        var result = await _service.CreateReservationAsync(reservation);
        _logger.LogInformation("ADMIN: Резервация создана — ID={Id}", result.SeatReservationId);
        return CreatedAtAction(nameof(GetReservation), new { id = result.SeatReservationId }, result);
    }

    [HttpPut("reservations/{id}")]
    public async Task<IActionResult> UpdateReservation(int id, [FromBody] SeatReservation reservation)
    {
        _logger.LogInformation("ADMIN: Обновление резервации — ID={Id}", id);
        var result = await _service.UpdateReservationAsync(id, reservation);
        if (result == null)
        {
            _logger.LogWarning("ADMIN: Резервация не найдена для обновления — ID={Id}", id);
            return NotFound();
        }
        _logger.LogInformation("ADMIN: Резервация обновлена — ID={Id}", id);
        return NoContent();
    }

    [HttpDelete("reservations/{id}")]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        _logger.LogInformation("ADMIN: Удаление резервации — ID={Id}", id);
        var success = await _service.DeleteReservationAsync(id);
        if (!success)
        {
            _logger.LogWarning("ADMIN: Резервация не найдена для удаления — ID={Id}", id);
            return NotFound();
        }
        _logger.LogInformation("ADMIN: Резервация удалена — ID={Id}", id);
        return NoContent();
    }
}