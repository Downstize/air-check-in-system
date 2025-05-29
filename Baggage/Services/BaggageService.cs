using System.Text.Json;
using Baggage.Data;
using Baggage.DTO;
using Baggage.Models;
using Microsoft.EntityFrameworkCore;
using Baggage.Clients;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Contracts;

namespace Baggage.Services
{
    public class BaggageService : IBaggageService
    {
        private readonly ApplicationDbContext _db;
        private readonly PassengerClient _passengerSvc;
        private readonly ILogger<BaggageService> _logger;
        private readonly IDistributedCache _cache;

        public BaggageService(ApplicationDbContext db,
            PassengerClient passengerSvc,
            ILogger<BaggageService> logger,
            IDistributedCache cache)
        {
            _db = db;
            _passengerSvc = passengerSvc;
            _logger = logger;
            _cache = cache;
        }

        public async Task<BaggageAllowanceDto> GetAllowanceAsync(string dynamicId, string orderId, string passengerId)
        {
            string cacheKey = $"allowance:{orderId}:{passengerId}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Возвращаем Allowance из кэша: {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<BaggageAllowanceDto>(cached)!;
            }

            _logger.LogInformation("Запрос Allowance из сервисов и БД: {OrderId}, {PassengerId}", orderId, passengerId);

            await Task.Delay(300);

            var order = await _passengerSvc.GetOrderAsync(dynamicId, orderId, passengerId);
            var passenger = order.Passengers.FirstOrDefault(p => p.PassengerId == passengerId);

            if (passenger == null)
            {
                _logger.LogWarning("Пассажир не найден в заказе: {PassengerId}", passengerId);
                throw new KeyNotFoundException("Пассажир не был найден в PassengerService");
            }

            var freeOption = await _db.PaidOptions
                .Where(po => po.Price == 0)
                .OrderBy(po => po.Pieces)
                .FirstOrDefaultAsync();

            int freePieces = freeOption?.Pieces ?? 1;
            decimal freeWeight = freeOption?.WeightKg ?? 20m;

            var paid = await _db.PaidOptions
                .Where(po => po.Price > 0)
                .Select(po => new PaidOptionDto { Pieces = po.Pieces, Weight = po.WeightKg, Price = po.Price })
                .ToListAsync();

            var result = new BaggageAllowanceDto
            {
                DynamicId = dynamicId,
                OrderId = orderId,
                PassengerId = passengerId,
                FreePieces = freePieces,
                FreeWeightKg = freeWeight,
                PaidOptions = paid
            };

            var serialized = JsonSerializer.Serialize(result);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            });

            _logger.LogInformation("Allowance закэширован: {CacheKey}", cacheKey);

            return result;
        }


        public async Task<BaggageRegistrationDto> RegisterAsync(string dynamicId, string orderId, string passengerId,
            int pieces, decimal weightKg)
        {
            _logger.LogInformation(
                "Вызов Baggage/RegisterAsync: DynamicId={DynamicId}, OrderId={OrderId}, PassengerId={PassengerId}, Pieces={Pieces}, WeightKg={WeightKg}",
                dynamicId, orderId, passengerId, pieces, weightKg);

            var order = await _passengerSvc.GetOrderAsync(dynamicId, orderId, passengerId);

            var passenger = order.Passengers.FirstOrDefault(p => p.PassengerId == passengerId);

            if (passenger == null)
            {
                _logger.LogWarning(
                    "Baggage/RegisterAsync: Пассажир не найден при регистрации багажа: PassengerId={PassengerId}",
                    passengerId);
                throw new KeyNotFoundException("Пассажир не был найден в PassengerService");
            }

            var existing = await _db.BaggageRegistrations.FirstOrDefaultAsync(r =>
                r.OrderId == orderId &&
                r.PassengerId == passengerId);

            if (existing != null)
            {
                _logger.LogWarning(
                    "Baggage/RegisterAsync: Попытка повторной регистрации багажа для PassengerId={PassengerId}",
                    passengerId);
                throw new InvalidOperationException("Багаж уже зарегистрирован для этого пассажира");
            }

            var paidOption = await _db.PaidOptions
                .OrderBy(po => po.Price)
                .FirstOrDefaultAsync();

            if (paidOption == null)
            {
                _logger.LogError("Baggage/RegisterAsync: Не найдены тарифы оплаты за багаж (PaidOptions)");
                throw new Exception("Не найдены тарифы оплаты за багаж");
            }

            var pricePerPiece = paidOption.Price;
            var price = pieces * pricePerPiece;

            if (price > 0)
            {
                var payment = await _db.BaggagePayment.FirstOrDefaultAsync(p =>
                    p.DynamicId == dynamicId &&
                    p.PassengerId == passengerId &&
                    p.DepartureId == order.Segments.First().DepartureId &&
                    p.Amount >= price &&
                    p.IsPaid);

                if (payment == null)
                {
                    _logger.LogWarning(
                        "Baggage/RegisterAsync: Оплата багажа не найдена или недостаточна: Требуется={Price}", price);
                    throw new Exception($"Оплата за багаж не найдена или недостаточная. Требуется {price}.");
                }
            }

            var reg = new BaggageRegistration
            {
                RegistrationId = Guid.NewGuid().ToString(),
                DynamicId = dynamicId,
                OrderId = orderId,
                PassengerId = passengerId,
                Pieces = pieces,
                WeightKg = weightKg,
                Price = price,
                TransactionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            _db.BaggageRegistrations.Add(reg);
            await _db.SaveChangesAsync();

            await _cache.RemoveAsync($"allowance:{orderId}:{passengerId}");
            _logger.LogInformation("Кэш Allowance сброшен после регистрации багажа: {CacheKey}",
                $"allowance:{orderId}:{passengerId}");

            _logger.LogInformation(
                "Baggage/RegisterAsync: Багаж успешно зарегистрирован: RegistrationId={RegistrationId}",
                reg.RegistrationId);

            return new BaggageRegistrationDto
            {
                DynamicId = dynamicId,
                OrderId = orderId,
                PassengerId = passengerId,
                Pieces = pieces,
                WeightKg = weightKg,
                Price = price
            };
        }

        public async Task<bool> CancelAsync(string dynamicId, string orderId, string passengerId)
        {
            _logger.LogInformation(
                "Вызов Baggage/CancelAsync: DynamicId={DynamicId}, OrderId={OrderId}, PassengerId={PassengerId}",
                dynamicId, orderId, passengerId);

            var order = await _passengerSvc.GetOrderAsync(dynamicId, orderId, passengerId);
            var passenger = order.Passengers.FirstOrDefault(p => p.PassengerId == passengerId);
            if (passenger == null)
            {
                _logger.LogWarning(
                    "Baggage/CancelAsync: Пассажир не найден при отмене багажа: PassengerId={PassengerId}",
                    passengerId);
                return false;
            }

            var regs = await _db.BaggageRegistrations
                .Where(r => r.OrderId == orderId && r.PassengerId == passengerId)
                .ToListAsync();

            if (!regs.Any())
            {
                _logger.LogWarning(
                    "Baggage/CancelAsync: Багажное бронирование не найдено для отмены: PassengerId={PassengerId}",
                    passengerId);
                return false;
            }

            _db.BaggageRegistrations.RemoveRange(regs);
            await _db.SaveChangesAsync();

            await _cache.RemoveAsync($"allowance:{orderId}:{passengerId}");
            _logger.LogInformation("Кэш Allowance сброшен после отмены багажа: {CacheKey}",
                $"allowance:{orderId}:{passengerId}");

            _logger.LogInformation("Регистрация багажа отменена для PassengerId={PassengerId}", passengerId);

            return true;
        }

        public async Task<bool> SimulateBaggagePaymentAsync(string dynamicId, string passengerId, string departureId,
            decimal amount)
        {
            _logger.LogInformation(
                "Вызов Baggage/SimulateBaggagePaymentAsync: Симуляция оплаты багажа: DynamicId={DynamicId}," +
                " PassengerId={PassengerId}, DepartureId={DepartureId}, Amount={Amount}",
                dynamicId, passengerId, departureId, amount);

            var payment = new BaggagePayment
            {
                PaymentId = Guid.NewGuid().ToString(),
                DynamicId = dynamicId,
                PassengerId = passengerId,
                DepartureId = departureId,
                Amount = amount,
                IsPaid = true,
                PaidAt = DateTime.UtcNow
            };
            _db.BaggagePayment.Add(payment);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Baggage/SimulateBaggagePaymentAsync: Оплата багажа успешно симулирована для PassengerId={PassengerId}",
                passengerId);

            return true;
        }

        public async Task<BaggagePayment> CreatePaymentAsync(BaggagePayment payment)
        {
            payment.PaymentId = Guid.NewGuid().ToString();
            _logger.LogInformation(
                "ADMIN (BaggageService): Создание оплаты — PassengerId={PassengerId}, Amount={Amount}",
                payment.PassengerId, payment.Amount);
            _db.BaggagePayment.Add(payment);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("payments:all");
            await _cache.RemoveAsync($"payment:{payment.PaymentId}");
            _logger.LogInformation("ADMIN (BaggageService): Оплата создана — PaymentId={PaymentId}", payment.PaymentId);
            return payment;
        }

        public async Task<IEnumerable<BaggagePayment>> GetAllPaymentsAsync()
        {
            const string cacheKey = "payments:all";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("ADMIN (BaggageService): Получение всех оплат из кэша");
                return JsonSerializer.Deserialize<IEnumerable<BaggagePayment>>(cached)!;
            }
            
            await Task.Delay(300);

            _logger.LogInformation("Запрос всех оплат из БД");
            var payments = await _db.BaggagePayment.ToListAsync();
            var serialized = JsonSerializer.Serialize(payments);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            return payments;
        }

        public async Task<BaggagePayment?> GetPaymentByIdAsync(string id)
        {
            var cacheKey = $"payment:{id}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("ADMIN (BaggageService): Оплата найдена в кэше: {PaymentId}", id);
                return JsonSerializer.Deserialize<BaggagePayment>(cached)!;
            }
            
            await Task.Delay(300);

            _logger.LogInformation("ADMIN (BaggageService): Получение оплаты по ID — PaymentId={PaymentId}", id);
            
            var payment = await _db.BaggagePayment.FindAsync(id);
            if (payment != null)
            {
                var serialized = JsonSerializer.Serialize(payment);
                await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }

            return payment;
        }

        public async Task<bool> UpdatePaymentAsync(string id, BaggagePayment updated)
        {
            _logger.LogInformation("ADMIN (BaggageService): Обновление оплаты — PaymentId={PaymentId}", id);
            var existing = await _db.BaggagePayment.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("ADMIN (BaggageService): Оплата не найдена — PaymentId={PaymentId}", id);
                return false;
            }

            updated.PaymentId = id;
            _db.Entry(existing).CurrentValues.SetValues(updated);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("payments:all");
            await _cache.RemoveAsync($"payment:{updated.PaymentId}");
            _logger.LogInformation("ADMIN (BaggageService): Оплата успешно обновлена — PaymentId={PaymentId}", id);
            return true;
        }

        public async Task<bool> DeletePaymentAsync(string id)
        {
            _logger.LogInformation("ADMIN (BaggageService): Удаление оплаты — PaymentId={PaymentId}", id);
            var existing = await _db.BaggagePayment.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("ADMIN (BaggageService): Оплата не найдена для удаления — PaymentId={PaymentId}",
                    id);
                return false;
            }

            _db.BaggagePayment.Remove(existing);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("payments:all");
            await _cache.RemoveAsync($"payment:{existing.PaymentId}");
            _logger.LogInformation("ADMIN (BaggageService): Оплата успешно удалена — PaymentId={PaymentId}", id);
            return true;
        }

        public async Task<BaggageRegistration> CreateRegistrationAsync(BaggageRegistration reg)
        {
            reg.RegistrationId = Guid.NewGuid().ToString();
            reg.Timestamp = DateTime.UtcNow;
            _logger.LogInformation(
                "ADMIN (BaggageService): Создание регистрации — PassengerId={PassengerId}, Pieces={Pieces}",
                reg.PassengerId, reg.Pieces);
            _db.BaggageRegistrations.Add(reg);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("registrations:all");
            await _cache.RemoveAsync($"registration:{reg.RegistrationId}");
            _logger.LogInformation("ADMIN (BaggageService): Регистрация создана — RegistrationId={RegistrationId}",
                reg.RegistrationId);
            return reg;
        }

        public async Task<IEnumerable<BaggageRegistration>> GetAllRegistrationsAsync()
        {
            const string cacheKey = "registrations:all";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("ADMIN (BaggageService): Получение всех регистраций из кэша");
                return JsonSerializer.Deserialize<IEnumerable<BaggageRegistration>>(cached)!;
            }
            
            await Task.Delay(300);

            _logger.LogInformation("ADMIN (BaggageService): Получение всех регистраций");
            
            var regs = await _db.BaggageRegistrations.ToListAsync();
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(regs),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            return regs;
        }

        public async Task<BaggageRegistration?> GetRegistrationByIdAsync(string id)
        {
            var cacheKey = $"registration:{id}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation(
                    "ADMIN (BaggageService): Получение регистрации по ID — RegistrationId={RegistrationId} из кэша", id);
                return JsonSerializer.Deserialize<BaggageRegistration>(cached)!;
            }
            
            await Task.Delay(300);

            _logger.LogInformation(
                "ADMIN (BaggageService): Получение регистрации по ID — RegistrationId={RegistrationId}", id);
            
            var reg = await _db.BaggageRegistrations.FindAsync(id);
            if (reg != null)
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(reg),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            return reg;
        }

        public async Task<bool> UpdateRegistrationAsync(string id, BaggageRegistration updated)
        {
            _logger.LogInformation("ADMIN (BaggageService): Обновление регистрации — RegistrationId={RegistrationId}",
                id);
            var existing = await _db.BaggageRegistrations.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("ADMIN (BaggageService): Регистрация не найдена — RegistrationId={RegistrationId}",
                    id);
                return false;
            }

            updated.RegistrationId = id;
            _db.Entry(existing).CurrentValues.SetValues(updated);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("registrations:all");
            await _cache.RemoveAsync($"registration:{id}");
            _logger.LogInformation(
                "ADMIN (BaggageService): Регистрация успешно обновлена — RegistrationId={RegistrationId}", id);
            return true;
        }

        public async Task<bool> DeleteRegistrationAsync(string id)
        {
            _logger.LogInformation("ADMIN (BaggageService): Удаление регистрации — RegistrationId={RegistrationId}",
                id);
            var existing = await _db.BaggageRegistrations.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning(
                    "ADMIN (BaggageService): Регистрация не найдена для удаления — RegistrationId={RegistrationId}",
                    id);
                return false;
            }

            _db.BaggageRegistrations.Remove(existing);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("registrations:all");
            await _cache.RemoveAsync($"registration:{id}");
            _logger.LogInformation(
                "ADMIN (BaggageService): Регистрация успешно удалена — RegistrationId={RegistrationId}", id);
            return true;
        }

        public async Task<PaidOption> CreatePaidOptionAsync(PaidOption option)
        {
            option.PaidOptionId = Guid.NewGuid().ToString();
            _logger.LogInformation(
                "ADMIN (BaggageService): Создание платной опции — Pieces={Pieces}, WeightKg={WeightKg}, Price={Price}",
                option.Pieces, option.WeightKg, option.Price);
            _db.PaidOptions.Add(option);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("paidoptions:all");
            await _cache.RemoveAsync($"paidoption:{option.PaidOptionId}");
            _logger.LogInformation("ADMIN (BaggageService): Опция создана — PaidOptionId={PaidOptionId}",
                option.PaidOptionId);
            return option;
        }

        public async Task<IEnumerable<PaidOption>> GetAllPaidOptionsAsync()
        {
            const string cacheKey = "paidoptions:all";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("ADMIN (BaggageService): Получение всех платных опций из кэша");
                return JsonSerializer.Deserialize<IEnumerable<PaidOption>>(cached)!;
            }
            
            await Task.Delay(300);

            _logger.LogInformation("ADMIN (BaggageService): Получение всех платных опций");
            
            var options = await _db.PaidOptions.ToListAsync();
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(options),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            return options;
        }

        public async Task<PaidOption?> GetPaidOptionByIdAsync(string id)
        {
            var cacheKey = $"paidoption:{id}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("ADMIN (BaggageService): Получение опции по ID — PaidOptionId={PaidOptionId} из кэша", id);
                return JsonSerializer.Deserialize<PaidOption>(cached)!;
            }
            
            await Task.Delay(300);

            _logger.LogInformation("ADMIN (BaggageService): Получение опции по ID — PaidOptionId={PaidOptionId}", id);
            
            var option = await _db.PaidOptions.FindAsync(id);
            if (option != null)
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(option),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            return option;
        }

        public async Task<bool> UpdatePaidOptionAsync(string id, PaidOption updated)
        {
            _logger.LogInformation("ADMIN (BaggageService): Обновление опции — PaidOptionId={PaidOptionId}", id);
            var existing = await _db.PaidOptions.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("ADMIN (BaggageService): Опция не найдена — PaidOptionId={PaidOptionId}", id);
                return false;
            }

            updated.PaidOptionId = id;
            _db.Entry(existing).CurrentValues.SetValues(updated);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("paidoptions:all");
            await _cache.RemoveAsync($"paidoption:{id}");
            _logger.LogInformation("ADMIN (BaggageService): Опция успешно обновлена — PaidOptionId={PaidOptionId}", id);
            return true;
        }

        public async Task<bool> DeletePaidOptionAsync(string id)
        {
            _logger.LogInformation("ADMIN (BaggageService): Удаление платной опции — PaidOptionId={PaidOptionId}", id);
            var existing = await _db.PaidOptions.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning(
                    "ADMIN (BaggageService): Опция не найдена для удаления — PaidOptionId={PaidOptionId}", id);
                return false;
            }

            _db.PaidOptions.Remove(existing);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("paidoptions:all");
            await _cache.RemoveAsync($"paidoption:{id}");
            _logger.LogInformation("ADMIN (BaggageService): Опция успешно удалена — PaidOptionId={PaidOptionId}", id);
            return true;
        }
    }
}