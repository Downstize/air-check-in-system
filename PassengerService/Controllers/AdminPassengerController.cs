using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PassengerService.Services;
using Shared.Contracts;

namespace PassengerService.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/admin/passengers")]
public class AdminPassengerController : ControllerBase
{
    private readonly IPassengersService _service;
    private readonly ILogger<AdminPassengerController> _logger;

    public AdminPassengerController(IPassengersService service, ILogger<AdminPassengerController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("ADMIN: Запрос списка всех пассажиров");
        var passengers = await _service.GetAllAsync();

        var result = passengers.Select(p => new PassengerDto
        {
            PassengerId = p.PassengerId,
            PnrId = p.PnrId,
            PaxNo = p.PaxNo,
            LastName = p.LastName,
            FirstName = p.FirstName,
            BirthDate = p.BirthDate,
            Category = p.Category,
            CheckInStatus = p.CheckInStatus,
            Reason = p.Reason,
            SeatsOccupied = p.SeatsOccupied,
            Eticket = p.Eticket,
            Document = new PassengerDocumentDto
            {
                Type = p.Document.Type,
                IssueCountryCode = p.Document.IssueCountryCode,
                Number = p.Document.Number,
                NationalityCode = p.Document.NationalityCode,
                BirthDate = p.Document.BirthDate,
                ExpiryDate = p.Document.ExpiryDate
            },
            VisaDocument = new PassengerVisaDocumentDto
            {
                BirthPlace = p.VisaDocument.BirthPlace,
                Number = p.VisaDocument.Number,
                IssuePlace = p.VisaDocument.IssuePlace,
                IssueDate = p.VisaDocument.IssueDate,
                ApplicCountryCode = p.VisaDocument.ApplicCountryCode
            },
            SeatNumber = p.SeatNumber,
            SeatStatus = p.SeatStatus,
            SeatLayerType = p.SeatLayerType,
            Remarks = p.Remarks,
            Apis = p.Apis,
            BookingId = p.BookingId
        }).ToList();

        _logger.LogInformation("ADMIN: Получено {Count} пассажиров", result.Count);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        _logger.LogInformation("ADMIN: Запрос пассажира по ID — {PassengerId}", id);
        var dto = await _service.GetByIdAsync(id);

        if (dto is null)
        {
            _logger.LogWarning("ADMIN: Пассажир не найден — ID={PassengerId}", id);
            return NotFound();
        }

        _logger.LogInformation("ADMIN: Пассажир найден — ID={PassengerId}", id);
        return Ok(dto);
    }



    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PassengerDto dto)
    {
        _logger.LogInformation("ADMIN: Создание пассажира — {LastName} {FirstName}", dto.LastName, dto.FirstName);

        dto.PassengerId ??= Guid.NewGuid().ToString();

        var created = await _service.CreateAsync(dto);

        _logger.LogInformation("ADMIN: Пассажир создан — ID={PassengerId}", created.PassengerId);
        return CreatedAtAction(nameof(GetById), new { id = created.PassengerId }, created);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] PassengerDto dto)
    {
        _logger.LogInformation("ADMIN: Обновление пассажира — ID={PassengerId}", id);

        if (id != dto.PassengerId)
        {
            _logger.LogWarning("ADMIN: ID в пути и в теле запроса не совпадают");
            return BadRequest("ID не совпадает");
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(kv => kv.Value?.Errors?.Count > 0)
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            _logger.LogWarning("ADMIN: Ошибка валидации модели: {@Errors}", errors);
            return BadRequest(ModelState);
        }


        var updated = await _service.UpdateAsync(id, dto);
        if (!updated)
        {
            _logger.LogWarning("ADMIN: Пассажир не найден — ID={PassengerId}", id);
            return NotFound();
        }

        _logger.LogInformation("ADMIN: Пассажир обновлён — ID={PassengerId}", id);
        return NoContent();
    }




    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.LogInformation("ADMIN: Удаление пассажира — ID={PassengerId}", id);
        var success = await _service.DeleteAsync(id);
        if (!success)
        {
            _logger.LogWarning("ADMIN: Удаление не выполнено — пассажир с ID={PassengerId} не найден", id);
            return NotFound();
        }

        _logger.LogInformation("ADMIN: Пассажир удалён — ID={PassengerId}", id);
        return NoContent();
    }
}