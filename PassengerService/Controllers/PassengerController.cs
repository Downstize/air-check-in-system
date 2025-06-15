using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PassengerService.Data;
using PassengerService.Models;
using Shared.Contracts;

namespace PassengerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PassengerController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<PassengerController> _logger;

    public PassengerController(ApplicationDbContext db, ILogger<PassengerController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Запрос на получение всех пассажиров.");
        var passengers = await _db.Passengers
            .Include(p => p.Document)
            .Include(p => p.VisaDocument)
            .ToListAsync();

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

        _logger.LogInformation("Получено {Count} пассажиров.", result.Count);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        _logger.LogInformation("Запрос на получение пассажира с ID: {PassengerId}", id);
        var p = await _db.Passengers
            .Include(p => p.Document)
            .Include(p => p.VisaDocument)
            .FirstOrDefaultAsync(p => p.PassengerId == id);

        if (p == null)
        {
            _logger.LogWarning("Пассажир с ID {PassengerId} не найден.", id);
            return NotFound();
        }

        var dto = new PassengerDto
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
        };

        _logger.LogInformation("Пассажир с ID {PassengerId} успешно найден.", id);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PassengerDto dto)
    {
        _logger.LogInformation("Создание нового пассажира.");
        var booking = await _db.Bookings.FindAsync(dto.BookingId);
        if (booking == null)
        {
            _logger.LogWarning("Бронирование с ID {BookingId} не найдено.", dto.BookingId);
            return BadRequest($"Booking с Id {dto.BookingId} не найден.");
        }

        var passenger = new Passenger
        {
            PassengerId = string.IsNullOrEmpty(dto.PassengerId) ? Guid.NewGuid().ToString() : dto.PassengerId,
            PnrId = dto.PnrId,
            PaxNo = dto.PaxNo,
            LastName = dto.LastName,
            FirstName = dto.FirstName,
            BirthDate = dto.BirthDate,
            Category = dto.Category,
            CheckInStatus = dto.CheckInStatus,
            Reason = dto.Reason,
            SeatsOccupied = dto.SeatsOccupied,
            Eticket = dto.Eticket,
            Document = new PassengerDocument
            {
                Type = dto.Document.Type,
                IssueCountryCode = dto.Document.IssueCountryCode,
                Number = dto.Document.Number,
                NationalityCode = dto.Document.NationalityCode,
                BirthDate = dto.Document.BirthDate,
                ExpiryDate = dto.Document.ExpiryDate
            },
            VisaDocument = new PassengerVisaDocument
            {
                BirthPlace = dto.VisaDocument.BirthPlace,
                Number = dto.VisaDocument.Number,
                IssuePlace = dto.VisaDocument.IssuePlace,
                IssueDate = dto.VisaDocument.IssueDate,
                ApplicCountryCode = dto.VisaDocument.ApplicCountryCode
            },
            SeatNumber = dto.SeatNumber,
            SeatStatus = dto.SeatStatus,
            SeatLayerType = dto.SeatLayerType,
            Remarks = dto.Remarks ?? new List<string>(),
            Apis = dto.Apis,
            BookingId = dto.BookingId,
            Booking = booking
        };

        _db.Passengers.Add(passenger);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Пассажир успешно создан с ID: {PassengerId}", passenger.PassengerId);

        var resultDto = new PassengerDto
        {
            PassengerId = passenger.PassengerId,
            PnrId = passenger.PnrId,
            PaxNo = passenger.PaxNo,
            LastName = passenger.LastName,
            FirstName = passenger.FirstName,
            BirthDate = passenger.BirthDate,
            Category = passenger.Category,
            CheckInStatus = passenger.CheckInStatus,
            Reason = passenger.Reason,
            SeatsOccupied = passenger.SeatsOccupied,
            Eticket = passenger.Eticket,
            Document = new PassengerDocumentDto
            {
                Type = passenger.Document.Type,
                IssueCountryCode = passenger.Document.IssueCountryCode,
                Number = passenger.Document.Number,
                NationalityCode = passenger.Document.NationalityCode,
                BirthDate = passenger.Document.BirthDate,
                ExpiryDate = passenger.Document.ExpiryDate
            },
            VisaDocument = new PassengerVisaDocumentDto
            {
                BirthPlace = passenger.VisaDocument.BirthPlace,
                Number = passenger.VisaDocument.Number,
                IssuePlace = passenger.VisaDocument.IssuePlace,
                IssueDate = passenger.VisaDocument.IssueDate,
                ApplicCountryCode = passenger.VisaDocument.ApplicCountryCode
            },
            SeatNumber = passenger.SeatNumber,
            SeatStatus = passenger.SeatStatus,
            SeatLayerType = passenger.SeatLayerType,
            Remarks = passenger.Remarks,
            Apis = passenger.Apis,
            BookingId = passenger.BookingId
        };

        return CreatedAtAction(nameof(Get), new { id = passenger.PassengerId }, resultDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, PassengerDto dto)
    {
        _logger.LogInformation("Обновление пассажира с ID: {PassengerId}", id);
        if (id != dto.PassengerId)
        {
            _logger.LogWarning("Id из пути не совпадает с Id из тела запроса.");
            return BadRequest("Id не совпадает.");
        }

        var passenger = await _db.Passengers
            .Include(p => p.Document)
            .Include(p => p.VisaDocument)
            .FirstOrDefaultAsync(p => p.PassengerId == id);

        if (passenger == null)
        {
            _logger.LogWarning("Пассажир с ID {PassengerId} не найден.", id);
            return NotFound();
        }

        var booking = await _db.Bookings.FindAsync(dto.BookingId);
        if (booking == null)
        {
            _logger.LogWarning("Бронирование с ID {BookingId} не найдено.", dto.BookingId);
            return BadRequest($"Booking с Id {dto.BookingId} не найден.");
        }

        passenger.PnrId = dto.PnrId;
        passenger.PaxNo = dto.PaxNo;
        passenger.LastName = dto.LastName;
        passenger.FirstName = dto.FirstName;
        passenger.BirthDate = dto.BirthDate;
        passenger.Category = dto.Category;
        passenger.CheckInStatus = dto.CheckInStatus;
        passenger.Reason = dto.Reason;
        passenger.SeatsOccupied = dto.SeatsOccupied;
        passenger.Eticket = dto.Eticket;
        passenger.SeatNumber = dto.SeatNumber;
        passenger.SeatStatus = dto.SeatStatus;
        passenger.SeatLayerType = dto.SeatLayerType;
        passenger.Remarks = dto.Remarks ?? new List<string>();
        passenger.Apis = dto.Apis;
        passenger.BookingId = dto.BookingId;
        passenger.Booking = booking;

        passenger.Document.Type = dto.Document.Type;
        passenger.Document.IssueCountryCode = dto.Document.IssueCountryCode;
        passenger.Document.Number = dto.Document.Number;
        passenger.Document.NationalityCode = dto.Document.NationalityCode;
        passenger.Document.BirthDate = dto.Document.BirthDate;
        passenger.Document.ExpiryDate = dto.Document.ExpiryDate;

        passenger.VisaDocument.BirthPlace = dto.VisaDocument.BirthPlace;
        passenger.VisaDocument.Number = dto.VisaDocument.Number;
        passenger.VisaDocument.IssuePlace = dto.VisaDocument.IssuePlace;
        passenger.VisaDocument.IssueDate = dto.VisaDocument.IssueDate;
        passenger.VisaDocument.ApplicCountryCode = dto.VisaDocument.ApplicCountryCode;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Пассажир с ID {PassengerId} успешно обновлён.", id);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.LogInformation("Запрос на удаление пассажира с ID: {PassengerId}", id);
        var passenger = await _db.Passengers.FirstOrDefaultAsync(p => p.PassengerId == id);
        if (passenger == null)
        {
            _logger.LogWarning("Пассажир с ID {PassengerId} не найден для удаления.", id);
            return NotFound();
        }

        _db.Passengers.Remove(passenger);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Пассажир с ID {PassengerId} успешно удалён.", id);
        return NoContent();
    }
}
