using AirCheckInOrchestrator.Api.DTO.Requests;
using Microsoft.AspNetCore.Authorization;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Prometheus;
using Shared.Contracts;
using Shared.Messages;

namespace AirCheckInOrchestrator.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrchestratorController : ControllerBase
{
    private readonly IRequestClient<BaggageRegistrationRequest> _bagClient;
    private readonly IRequestClient<RegistrationCheckInRequest> _regClient;
    private readonly ILogger<OrchestratorController> _logger;
    private readonly Counter _checkInCounter;
    private readonly Counter _baggageCounter;

    public OrchestratorController(
        IRequestClient<BaggageRegistrationRequest> bagClient,
        IRequestClient<RegistrationCheckInRequest> regClient,
        ILogger<OrchestratorController> logger,
        [FromKeyedServices("checkIn")] Counter checkInCounter,
        [FromKeyedServices("baggage")] Counter baggageCounter)
    {
        _bagClient = bagClient;
        _regClient = regClient;
        _logger = logger;
        _checkInCounter = checkInCounter;
        _baggageCounter = baggageCounter;
    }

    [HttpPost("checkin")]
    public async Task<ActionResult<OrderDto>> CheckIn([FromBody] CheckInRequest req)
    {
        _logger.LogInformation("Получен запрос на регистрацию пассажира {@Request}", req);

        try
        {
            var response = await _regClient.GetResponse<RegistrationCheckInResponse>(new RegistrationCheckInRequest
            {
                DynamicId = req.DynamicId,
                LastName = req.LastName,
                Pnr = req.Pnr,
                PaidSeat = req.PaidSeat
            });

            _logger.LogInformation("Пассажир успешно зарегистрирован. OrderId={OrderId}", response.Message.Order.OrderId);
            
            _checkInCounter.Inc();

            return Ok(response.Message.Order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации пассажира с PNR={Pnr}, LastName={LastName}", req.Pnr, req.LastName);
            return StatusCode(500, "Произошла ошибка при регистрации пассажира");
        }
    }

    [HttpPost("baggage")]
    public async Task<ActionResult<BaggageRegistrationDto>> AddBaggage([FromBody] BaggageRequest req)
    {
        _logger.LogInformation("Получен запрос на регистрацию багажа {@Request}", req);

        try
        {
            var response = await _bagClient.GetResponse<BaggageRegistrationResponse>(new BaggageRegistrationRequest
            {
                DynamicId = req.DynamicId,
                Pnr = req.Pnr,
                PassengerId = req.PassengerId,
                Pieces = req.Pieces,
                Weight = req.Weight
            });

            _logger.LogInformation("Багаж успешно зарегистрирован. PassengerId={PassengerId}, Pieces={Pieces}, Weight={Weight}",
                req.PassengerId, req.Pieces, req.Weight);
            
            _baggageCounter.Inc();

            return Ok(response.Message.Registration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации багажа для PassengerId={PassengerId}", req.PassengerId);
            return StatusCode(500, "Произошла ошибка при регистрации багажа");
        }
    }
}