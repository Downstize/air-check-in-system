using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Registration.DTO.Auth;
using Registration.DTO.Order;
using Registration.DTO.Registration;
using Registration.Models;
using Registration.Services;

namespace Registration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _svc;
        private readonly RegistrationAuthOptions _authOptions;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(
            IRegistrationService svc, 
            IOptions<RegistrationAuthOptions> authOptions,
            ILogger<RegistrationController> logger)
        {
            _svc = svc;
            _authOptions = authOptions.Value;
            _logger = logger;
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<RegistrationAuthResponseDto>> Authenticate([FromBody] RegistrationAuthRequestDto req)
        {
            _logger.LogInformation("Попытка аутентификации: Login={Login}", req.Login);

            if (req.Login != _authOptions.Login || req.Pwd != _authOptions.Password)
            {
                _logger.LogWarning("Недопустимый логин или пароль: Login={Login}", req.Login);
                return Unauthorized("Недопустимый логин или пароль");
            }

            var result = await _svc.AuthenticateAsync(req);
            _logger.LogInformation("Аутентификация прошла успешно: Login={Login}", req.Login);
            return Ok(result);
        }

        [HttpPost("order")]
        public async Task<ActionResult<RegistrationOrderSearchResponseDto>> SearchOrder([FromBody] RegistrationOrderSearchRequestDto req)
        {
            _logger.LogInformation("Поиск заказа: PNR={PnrId}, LastName={LastName}", req.PnrId, req.LastName);
            var result = await _svc.SearchOrderAsync(req);
            _logger.LogInformation("Заказ найден: OrderId={OrderId}", result.Order.OrderId);
            return Ok(result);
        }

        [HttpPost("reserve")]
        public async Task<ActionResult<RegistrationSeatReserveResponseDto>> Reserve([FromBody] RegistrationSeatReserveRequestDto req)
        {
            _logger.LogInformation("Резервирование места: PassengerId={PassengerId}, DepartureId={DepartureId}", req.PassengerId, req.DepartureId);
            var result = await _svc.ReserveSeatAsync(req);
            _logger.LogInformation("Место зарезервировано: {SeatNumber}", result.Seat.SeatNumber);
            return Ok(result);
        }

        [HttpPost("registerFree")]
        public async Task<ActionResult<RegistrationPassengerResponseDto>> RegisterFree([FromBody] RegistrationPassengerFreeRequestDto req)
        {
            _logger.LogInformation("Бесплатная регистрация пассажира: PassengerId={PassengerId}", req.PassengerId);
            var result = await _svc.RegisterFreeAsync(req);
            _logger.LogInformation("Регистрация завершена. Место: {SeatNumber}", result.SeatNumber);
            return Ok(result);
        }

        [HttpPost("registerPaid")]
        public async Task<ActionResult<RegistrationPassengerResponseDto>> RegisterPaid([FromBody] RegistrationPassengerPaidRequestDto req)
        {
            _logger.LogInformation("Платная регистрация пассажира: PassengerId={PassengerId}", req.PassengerId);
            var result = await _svc.RegisterPaidAsync(req);
            _logger.LogInformation("Регистрация завершена. Место: {SeatNumber}", result.SeatNumber);
            return Ok(result);
        }

        [HttpPost("simulatePayment")]
        public async Task<IActionResult> SimulatePayment(string dynamicId, string passengerId, string departureId, decimal amount)
        {
            _logger.LogInformation("Симуляция оплаты: PassengerId={PassengerId}, Amount={Amount}", passengerId, amount);
            var success = await _svc.SimulatePaymentAsync(dynamicId, passengerId, departureId, amount);

            if (success)
            {
                _logger.LogInformation("Оплата успешно проведена: PassengerId={PassengerId}", passengerId);
                return Ok("Оплата успешно проведена.");
            }
            else
            {
                _logger.LogWarning("Ошибка при оплате: PassengerId={PassengerId}", passengerId);
                return BadRequest("Ошибка оплаты.");
            }
        }
    }
}
