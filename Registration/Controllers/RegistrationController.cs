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
        public RegistrationController(IRegistrationService svc, IOptions<RegistrationAuthOptions> authOptions)
        {
            _svc = svc;
            _authOptions = authOptions.Value;
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<RegistrationAuthResponseDto>> Authenticate([FromBody] RegistrationAuthRequestDto req)
        {
            if (req.Login != _authOptions.Login || req.Pwd != _authOptions.Password)
                return Unauthorized("Недопустимый логин или пароль");

            return Ok(await _svc.AuthenticateAsync(req));
        }

        [HttpPost("order")]
        public async Task<ActionResult<RegistrationOrderSearchResponseDto>> SearchOrder([FromBody] RegistrationOrderSearchRequestDto req)
            => Ok(await _svc.SearchOrderAsync(req));

        [HttpPost("reserve")]
        public async Task<ActionResult<RegistrationSeatReserveResponseDto>> Reserve([FromBody] RegistrationSeatReserveRequestDto req)
            => Ok(await _svc.ReserveSeatAsync(req));

        [HttpPost("registerFree")]
        public async Task<ActionResult<RegistrationPassengerResponseDto>> RegisterFree([FromBody] RegistrationPassengerFreeRequestDto req)
            => Ok(await _svc.RegisterFreeAsync(req));

        [HttpPost("registerPaid")]
        public async Task<ActionResult<RegistrationPassengerResponseDto>> RegisterPaid([FromBody] RegistrationPassengerPaidRequestDto req)
            => Ok(await _svc.RegisterPaidAsync(req));
        
        [HttpPost("simulatePayment")]
        public async Task<IActionResult> SimulatePayment(string dynamicId, string passengerId, string departureId, decimal amount)
        {
            var success = await _svc.SimulatePaymentAsync(dynamicId, passengerId, departureId, amount);
            return success ? Ok("Оплата успешно проведена.") : BadRequest("Ошибка оплаты.");
        }

    }
}