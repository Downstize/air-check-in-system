using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PassengerService.Data;
using PassengerService.Models;
using Shared.Contracts;

namespace PassengerService.Services;

public class PassengersService : IPassengersService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<PassengersService> _logger;
    private readonly IDistributedCache _cache;

    public PassengersService(ApplicationDbContext db,
        ILogger<PassengersService> logger,
        IDistributedCache cache)
    {
        _db = db;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<PassengerDto>> GetAllAsync()
{
    var cached = await _cache.GetStringAsync("passenger:all");
    if (cached != null)
    {
        _logger.LogInformation("ADMIN (PassengerService): Получение всех пассажиров из кэша");
        return JsonSerializer.Deserialize<IEnumerable<PassengerDto>>(cached)!;
    }

    await Task.Delay(300);

    _logger.LogInformation("ADMIN (PassengerService): Запрос всех пассажиров из БД");
    var passengers = await _db.Passengers
        .Include(p => p.Document)
        .Include(p => p.VisaDocument)
        .ToListAsync();

    var dtos = passengers.Select(p => new PassengerDto
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
        SeatNumber = p.SeatNumber,
        SeatStatus = p.SeatStatus,
        SeatLayerType = p.SeatLayerType,
        Remarks = p.Remarks,
        Apis = p.Apis,
        BookingId = p.BookingId,
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
        }
    }).ToList();

    var serialized = JsonSerializer.Serialize(dtos);
    await _cache.SetStringAsync("passenger:all", serialized, new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    });

    return dtos;
}


    public async Task<PassengerDto?> GetByIdAsync(string id)
{
    string cacheKey = $"passenger:{id}";
    var cached = await _cache.GetStringAsync(cacheKey);
    if (cached != null)
    {
        _logger.LogInformation("ADMIN (PassengerService): Пассажир {Id} получен из кэша", id);
        return JsonSerializer.Deserialize<PassengerDto>(cached);
    }

    await Task.Delay(300);

    _logger.LogInformation("ADMIN (PassengerService): Запрос пассажира {Id} из БД", id);
    var p = await _db.Passengers
        .Include(p => p.Document)
        .Include(p => p.VisaDocument)
        .FirstOrDefaultAsync(p => p.PassengerId == id);

    if (p == null) return null;

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
        SeatNumber = p.SeatNumber,
        SeatStatus = p.SeatStatus,
        SeatLayerType = p.SeatLayerType,
        Remarks = p.Remarks,
        Apis = p.Apis,
        BookingId = p.BookingId,
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
        }
    };

    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    });

    return dto;
}


    public async Task<PassengerDto> CreateAsync(PassengerDto dto)
    {
        _logger.LogInformation("ADMIN (PassengerService): Создание пассажира {Id}", dto.PassengerId);
        
        var passenger = new Passenger
        {
            PassengerId = dto.PassengerId,
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
            SeatNumber = dto.SeatNumber,
            SeatStatus = dto.SeatStatus,
            SeatLayerType = dto.SeatLayerType,
            Remarks = dto.Remarks ?? new List<string>(),
            Apis = dto.Apis,
            BookingId = dto.BookingId,
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
            }
        };

        _db.Passengers.Add(passenger);
        await _db.SaveChangesAsync();

        await _cache.RemoveAsync("passenger:all");
        _logger.LogInformation("ADMIN (PassengerService): Общий кэш пассажиров удалён");

        return dto;
    }

    public async Task<bool> UpdateAsync(string id, PassengerDto dto)
    {
        var existing = await _db.Passengers
            .Include(p => p.Document)
            .Include(p => p.VisaDocument)
            .FirstOrDefaultAsync(p => p.PassengerId == id);

        if (existing == null)
        {
            _logger.LogWarning("ADMIN (PassengerService): Не найден пассажир для обновления — ID={Id}", id);
            return false;
        }

        existing.PnrId = dto.PnrId;
        existing.PaxNo = dto.PaxNo;
        existing.LastName = dto.LastName;
        existing.FirstName = dto.FirstName;
        existing.BirthDate = dto.BirthDate;
        existing.Category = dto.Category;
        existing.CheckInStatus = dto.CheckInStatus;
        existing.Reason = dto.Reason;
        existing.SeatsOccupied = dto.SeatsOccupied;
        existing.Eticket = dto.Eticket;
        existing.SeatNumber = dto.SeatNumber;
        existing.SeatStatus = dto.SeatStatus;
        existing.SeatLayerType = dto.SeatLayerType;
        existing.Remarks = dto.Remarks ?? new List<string>();
        existing.Apis = dto.Apis;
        existing.BookingId = dto.BookingId;

        existing.Document = new PassengerDocument
        {
            Type = dto.Document.Type,
            IssueCountryCode = dto.Document.IssueCountryCode,
            Number = dto.Document.Number,
            NationalityCode = dto.Document.NationalityCode,
            BirthDate = dto.Document.BirthDate,
            ExpiryDate = dto.Document.ExpiryDate
        };

        existing.VisaDocument = new PassengerVisaDocument
        {
            BirthPlace = dto.VisaDocument.BirthPlace,
            Number = dto.VisaDocument.Number,
            IssuePlace = dto.VisaDocument.IssuePlace,
            IssueDate = dto.VisaDocument.IssueDate,
            ApplicCountryCode = dto.VisaDocument.ApplicCountryCode
        };

        await _db.SaveChangesAsync();

        await _cache.RemoveAsync($"passenger:{id}");
        await _cache.RemoveAsync("passenger:all");
        
        _logger.LogInformation("ADMIN (PassengerService): Пассажир {Id} обновлён и кэш очищен", id);

        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var existing = await _db.Passengers.FirstOrDefaultAsync(p => p.PassengerId == id);
        if (existing == null)
        {
            _logger.LogWarning("ADMIN (PassengerService): Не найден пассажир для удаления — ID={Id}", id);
            return false;
        }

        _db.Passengers.Remove(existing);
        await _db.SaveChangesAsync();

        await _cache.RemoveAsync($"passenger:{id}");
        await _cache.RemoveAsync("passenger:all");
        _logger.LogInformation("ADMIN (PassengerService): Пассажир {Id} удалён и кэш очищен", id);
        return true;
    }
}
