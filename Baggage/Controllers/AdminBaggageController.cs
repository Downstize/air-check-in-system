using Baggage.Models;
using Baggage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baggage.Controllers
{
    
    [Authorize(Policy = "AdminOnly")]
    [ApiController]
    [Route("api/admin/baggage")]
    public class AdminBaggageController : ControllerBase
    {
        private readonly IBaggageService _svc;
        private readonly ILogger<AdminBaggageController> _logger;

        public AdminBaggageController(IBaggageService svc, ILogger<AdminBaggageController> logger)
        {
            _svc = svc;
            _logger = logger;
        }
        
        [HttpPost("payments")]
        public async Task<ActionResult<BaggagePayment>> CreatePayment([FromBody] BaggagePayment payment)
        {
            _logger.LogInformation("ADMIN: Создание оплаты багажа — PassengerId={PassengerId}, Amount={Amount}", payment.PassengerId, payment.Amount);
            var result = await _svc.CreatePaymentAsync(payment);
            _logger.LogInformation("ADMIN: Оплата создана — PaymentId={PaymentId}", result.PaymentId);
            return Ok(result);
        }

        [HttpGet("payments")]
        public async Task<ActionResult<IEnumerable<BaggagePayment>>> GetAllPayments()
        {
            _logger.LogInformation("ADMIN: Получение всех оплат за багаж");
            return Ok(await _svc.GetAllPaymentsAsync());
        }

        [HttpGet("payments/{id}")]
        public async Task<ActionResult<BaggagePayment>> GetPaymentById(string id)
        {
            _logger.LogInformation("ADMIN: Получение оплаты по ID — PaymentId={PaymentId}", id);
            var result = await _svc.GetPaymentByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("ADMIN: Оплата не найдена — PaymentId={PaymentId}", id);
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPut("payments/{id}")]
        public async Task<IActionResult> UpdatePayment(string id, [FromBody] BaggagePayment updated)
        {
            _logger.LogInformation("ADMIN: Обновление оплаты — PaymentId={PaymentId}", id);
            var success = await _svc.UpdatePaymentAsync(id, updated);
            if (!success)
            {
                _logger.LogWarning("ADMIN: Не удалось обновить оплату — PaymentId={PaymentId}", id);
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("payments/{id}")]
        public async Task<IActionResult> DeletePayment(string id)
        {
            _logger.LogInformation("ADMIN: Удаление оплаты — PaymentId={PaymentId}", id);
            var success = await _svc.DeletePaymentAsync(id);
            if (!success)
            {
                _logger.LogWarning("ADMIN: Оплата для удаления не найдена — PaymentId={PaymentId}", id);
                return NotFound();
            }
            return NoContent();
        }
        
        [HttpPost("registrations")]
        public async Task<ActionResult<BaggageRegistration>> CreateRegistration([FromBody] BaggageRegistration reg)
        {
            _logger.LogInformation("ADMIN: Создание регистрации багажа — PassengerId={PassengerId}, Pieces={Pieces}", reg.PassengerId, reg.Pieces);
            var result = await _svc.CreateRegistrationAsync(reg);
            _logger.LogInformation("ADMIN: Регистрация создана — RegistrationId={RegistrationId}", result.RegistrationId);
            return Ok(result);
        }

        [HttpGet("registrations")]
        public async Task<ActionResult<IEnumerable<BaggageRegistration>>> GetAllRegistrations()
        {
            _logger.LogInformation("ADMIN: Получение всех регистраций багажа");
            return Ok(await _svc.GetAllRegistrationsAsync());
        }

        [HttpGet("registrations/{id}")]
        public async Task<ActionResult<BaggageRegistration>> GetRegistrationById(string id)
        {
            _logger.LogInformation("ADMIN: Получение регистрации по ID — RegistrationId={RegistrationId}", id);
            var result = await _svc.GetRegistrationByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("ADMIN: Регистрация не найдена — RegistrationId={RegistrationId}", id);
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPut("registrations/{id}")]
        public async Task<IActionResult> UpdateRegistration(string id, [FromBody] BaggageRegistration updated)
        {
            _logger.LogInformation("ADMIN: Обновление регистрации — RegistrationId={RegistrationId}", id);
            var success = await _svc.UpdateRegistrationAsync(id, updated);
            if (!success)
            {
                _logger.LogWarning("ADMIN: Не удалось обновить регистрацию — RegistrationId={RegistrationId}", id);
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("registrations/{id}")]
        public async Task<IActionResult> DeleteRegistration(string id)
        {
            _logger.LogInformation("ADMIN: Удаление регистрации — RegistrationId={RegistrationId}", id);
            var success = await _svc.DeleteRegistrationAsync(id);
            if (!success)
            {
                _logger.LogWarning("ADMIN: Регистрация для удаления не найдена — RegistrationId={RegistrationId}", id);
                return NotFound();
            }
            return NoContent();
        }
        
        [HttpPost("options")]
        public async Task<ActionResult<PaidOption>> CreateOption([FromBody] PaidOption option)
        {
            _logger.LogInformation("ADMIN: Создание платной опции — Pieces={Pieces}, WeightKg={WeightKg}, Price={Price}", option.Pieces, option.WeightKg, option.Price);
            var result = await _svc.CreatePaidOptionAsync(option);
            _logger.LogInformation("ADMIN: Платная опция создана — PaidOptionId={PaidOptionId}", result.PaidOptionId);
            return Ok(result);
        }

        [HttpGet("options")]
        public async Task<ActionResult<IEnumerable<PaidOption>>> GetAllOptions()
        {
            _logger.LogInformation("ADMIN: Получение всех платных опций");
            return Ok(await _svc.GetAllPaidOptionsAsync());
        }

        [HttpGet("options/{id}")]
        public async Task<ActionResult<PaidOption>> GetOptionById(string id)
        {
            _logger.LogInformation("ADMIN: Получение платной опции по ID — PaidOptionId={PaidOptionId}", id);
            var result = await _svc.GetPaidOptionByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("ADMIN: Платная опция не найдена — PaidOptionId={PaidOptionId}", id);
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPut("options/{id}")]
        public async Task<IActionResult> UpdateOption(string id, [FromBody] PaidOption updated)
        {
            _logger.LogInformation("ADMIN: Обновление платной опции — PaidOptionId={PaidOptionId}", id);
            var success = await _svc.UpdatePaidOptionAsync(id, updated);
            if (!success)
            {
                _logger.LogWarning("ADMIN: Не удалось обновить опцию — PaidOptionId={PaidOptionId}", id);
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("options/{id}")]
        public async Task<IActionResult> DeleteOption(string id)
        {
            _logger.LogInformation("ADMIN: Удаление платной опции — PaidOptionId={PaidOptionId}", id);
            var success = await _svc.DeletePaidOptionAsync(id);
            if (!success)
            {
                _logger.LogWarning("ADMIN: Опция для удаления не найдена — PaidOptionId={PaidOptionId}", id);
                return NotFound();
            }
            return NoContent();
        }
    }
}
