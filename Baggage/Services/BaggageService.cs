using Baggage.Data;
using Baggage.DTO;
using Baggage.Models;
using Microsoft.EntityFrameworkCore;
using Baggage.Clients;
using Shared.Contracts;

namespace Baggage.Services
{
    public class BaggageService : IBaggageService
    {
        private readonly ApplicationDbContext _db;
        private readonly PassengerClient _passengerSvc;
        private readonly ILogger<BaggageService> _logger;

        public BaggageService(ApplicationDbContext db,
            PassengerClient passengerSvc,
            ILogger<BaggageService> logger)
        {
            _db = db;
            _passengerSvc = passengerSvc;
            _logger = logger;
        }

        public async Task<BaggageAllowanceDto> GetAllowanceAsync(string dynamicId, string orderId, string passengerId)
        {
            _logger.LogInformation("Вызов Baggage/GetAllowanceAsync: DynamicId={DynamicId}, OrderId={OrderId}, PassengerId={PassengerId}",
                dynamicId, orderId, passengerId);
            
            var order = await _passengerSvc.GetOrderAsync(dynamicId, orderId, passengerId);
            
            var passenger = order.Passengers.FirstOrDefault(p => p.PassengerId == passengerId);
            
            if (passenger == null)
            {
                _logger.LogWarning("Baggage/GetAllowanceAsync: Пассажир не найден в заказе: PassengerId={PassengerId}", passengerId);
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

            _logger.LogInformation("Baggage/GetAllowanceAsync: Разрешение на багаж получено для PassengerId={PassengerId}", passengerId);
            
            return new BaggageAllowanceDto
            {
                DynamicId = dynamicId,
                OrderId = orderId,
                PassengerId = passengerId,
                FreePieces = freePieces,
                FreeWeightKg = freeWeight,
                PaidOptions = paid
            };
        }

        public async Task<BaggageRegistrationDto> RegisterAsync(string dynamicId, string orderId, string passengerId, int pieces, decimal weightKg)
        {
            _logger.LogInformation("Вызов Baggage/RegisterAsync: DynamicId={DynamicId}, OrderId={OrderId}, PassengerId={PassengerId}, Pieces={Pieces}, WeightKg={WeightKg}",
                dynamicId, orderId, passengerId, pieces, weightKg);
            
            var order = await _passengerSvc.GetOrderAsync(dynamicId, orderId, passengerId);
            
            var passenger = order.Passengers.FirstOrDefault(p => p.PassengerId == passengerId);
            
            if (passenger == null)
            {
                _logger.LogWarning("Baggage/RegisterAsync: Пассажир не найден при регистрации багажа: PassengerId={PassengerId}", passengerId);
                throw new KeyNotFoundException("Пассажир не был найден в PassengerService");
            }
            
            var existing = await _db.BaggageRegistrations.FirstOrDefaultAsync(r =>
                r.OrderId == orderId &&
                r.PassengerId == passengerId);

            if (existing != null)
            {
                _logger.LogWarning("Baggage/RegisterAsync: Попытка повторной регистрации багажа для PassengerId={PassengerId}", passengerId);
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
                    _logger.LogWarning("Baggage/RegisterAsync: Оплата багажа не найдена или недостаточна: Требуется={Price}", price);
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

            _logger.LogInformation("Baggage/RegisterAsync: Багаж успешно зарегистрирован: RegistrationId={RegistrationId}", reg.RegistrationId);
            
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
            _logger.LogInformation("Вызов Baggage/CancelAsync: DynamicId={DynamicId}, OrderId={OrderId}, PassengerId={PassengerId}",
                dynamicId, orderId, passengerId);
            
            var order = await _passengerSvc.GetOrderAsync(dynamicId, orderId, passengerId);
            var passenger = order.Passengers.FirstOrDefault(p => p.PassengerId == passengerId);
            if (passenger == null)
            {
                _logger.LogWarning("Baggage/CancelAsync: Пассажир не найден при отмене багажа: PassengerId={PassengerId}", passengerId);
                return false;
            }

            var regs = await _db.BaggageRegistrations
                .Where(r => r.OrderId == orderId && r.PassengerId == passengerId)
                .ToListAsync();

            if (!regs.Any())
            {
                _logger.LogWarning("Baggage/CancelAsync: Багажное бронирование не найдено для отмены: PassengerId={PassengerId}", passengerId);
                return false;
            }

            _db.BaggageRegistrations.RemoveRange(regs);
            await _db.SaveChangesAsync();
            
            _logger.LogInformation("Регистрация багажа отменена для PassengerId={PassengerId}", passengerId);
            
            return true;
        }
        
        public async Task<bool> SimulateBaggagePaymentAsync(string dynamicId, string passengerId, string departureId, decimal amount)
        {
            _logger.LogInformation("Вызов Baggage/SimulateBaggagePaymentAsync: Симуляция оплаты багажа: DynamicId={DynamicId}," +
                                   " PassengerId={PassengerId}, DepartureId={DepartureId}, Amount={Amount}",
                dynamicId, passengerId, departureId, amount);
            
            var payment = new BaggagePayment
            {
                DynamicId = dynamicId,
                PassengerId = passengerId,
                DepartureId = departureId,
                Amount = amount,
                IsPaid = true,
                PaidAt = DateTime.UtcNow
            };
            _db.BaggagePayment.Add(payment);
            await _db.SaveChangesAsync();
            
            _logger.LogInformation("Baggage/SimulateBaggagePaymentAsync: Оплата багажа успешно симулирована для PassengerId={PassengerId}", passengerId);
            
            return true;
        }

    }
}
