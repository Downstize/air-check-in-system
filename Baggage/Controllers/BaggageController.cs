using Baggage.DTO;
using Baggage.DTO.Requests;
using Baggage.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Messages;

namespace Baggage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaggageController : ControllerBase
    {
        private readonly IBaggageService _svc;
        private readonly IRequestClient<ValidateDynamicIdRequest> _sessionClient;
        private readonly ILogger<BaggageController> _logger;
        
        public BaggageController(IBaggageService svc,
            IRequestClient<ValidateDynamicIdRequest> sessionClient,
            ILogger<BaggageController> logger)
        {
            _svc = svc;
            _sessionClient = sessionClient;
            _logger = logger;
        }

        [HttpGet("allowance")]
        public async Task<ActionResult<BaggageAllowanceDto>> GetAllowance(
            [FromQuery]string dynamicId,
            [FromQuery]string orderId,
            [FromQuery]string passengerId)
        {
            _logger.LogInformation("Запрос разрешения на багаж: DynamicId={DynamicId}, OrderId={OrderId}, PassengerId={PassengerId}",
                dynamicId, orderId, passengerId);
            
            var validation = await ValidateDynamicIdAsync(dynamicId);
            if (validation != null)
            {
                _logger.LogWarning("Неверный DynamicId: {DynamicId}", dynamicId);
                return new ObjectResult(validation) { StatusCode = 401 };
            }

            var allowance = await _svc.GetAllowanceAsync(dynamicId, orderId, passengerId);

            _logger.LogInformation("Разрешение на багаж успешно получено для PassengerId={PassengerId}", passengerId);

            return Ok(allowance);
        }

        [HttpPost("register")]
        public async Task<ActionResult<BaggageRegistrationDto>> Register([FromBody]RegisterRequest req)
        {
            _logger.LogInformation("Запрос на регистрацию багажа: {@Request}", req);
            
            var validation = await ValidateDynamicIdAsync(req.DynamicId);
            if (validation != null)
            {
                _logger.LogWarning("Неверный DynamicId: {DynamicId}", req.DynamicId);
                return new ObjectResult(validation) { StatusCode = 401 };
            }

            var dto = await _svc.RegisterAsync(req.DynamicId, req.OrderId, req.PassengerId, req.Pieces, req.WeightKg);
            
            _logger.LogInformation("Багаж успешно зарегистрирован для: PassengerId={PassengerId}", dto.PassengerId);
            
            return Ok(dto);
        }

        [HttpPost("cancel")]
        public async Task<ActionResult> Cancel([FromBody]CancelRequest req)
        {
            _logger.LogInformation("Запрос на отмену багажа: {@Request}", req);
            
            var validation = await ValidateDynamicIdAsync(req.DynamicId);
            if (validation != null)
            {
                _logger.LogWarning("Неверный DynamicId: {DynamicId}", req.DynamicId);
                return new ObjectResult(validation) { StatusCode = 401 };
            }

            var ok = await _svc.CancelAsync(req.DynamicId, req.OrderId, req.PassengerId);
            
            if (!ok)
            {
                _logger.LogWarning("Не найдено багажное бронирование для отмены: OrderId={OrderId}, PassengerId={PassengerId}",
                    req.OrderId, req.PassengerId);
                return NotFound();
            }

            _logger.LogInformation("Регистрация багажа отменена для PassengerId={PassengerId}", req.PassengerId);

            return NoContent();
        }
        
        [HttpPost("simulatePayment")]
        public async Task<IActionResult> SimulateBaggagePayment(string dynamicId, string passengerId, string departureId, decimal amount)
        {
            _logger.LogInformation("Запрос на симуляцию оплаты багажа: DynamicId={DynamicId}, PassengerId={PassengerId}, Amount={Amount}",
                dynamicId, passengerId, amount);
            
            var validation = await ValidateDynamicIdAsync(dynamicId);
            if (validation != null)
            {
                _logger.LogWarning("Неверный DynamicId: {DynamicId}", dynamicId);
                return new ObjectResult(validation) { StatusCode = 401 };
            }

            var success = await _svc.SimulateBaggagePaymentAsync(dynamicId, passengerId, departureId, amount);
            
            if (success)
            {
                _logger.LogInformation("Оплата багажа успешно проведена для PassengerId={PassengerId}", passengerId);
                return Ok("Оплата багажа успешно проведена.");
            }

            _logger.LogWarning("Ошибка оплаты багажа для PassengerId={PassengerId}", passengerId);
            return BadRequest("Ошибка оплаты багажа.");

        }
        
        private async Task<IActionResult?> ValidateDynamicIdAsync(string dynamicId)
        {
            _logger.LogInformation("Отправляем запрос на проверку идентификатора: {@DynamicId}", dynamicId);
            
            var sessionResponse = await _sessionClient.GetResponse<ValidateDynamicIdResponse>(
                new ValidateDynamicIdRequest { DynamicId = dynamicId });

            if (!sessionResponse.Message.IsValid)
            {
                _logger.LogInformation("Недействительный dynamicId: {@DynamicId}", dynamicId);
                return Unauthorized("Недействительный dynamicId");
            }

            return null;
        }
    }
}
