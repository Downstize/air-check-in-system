using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Registration.Clients;
using Registration.Data;
using Registration.DTO.Auth;
using Registration.DTO.Order;
using Registration.DTO.Registration;
using Registration.Models;
using Shared.Messages;

namespace Registration.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ApplicationDbContext _db;
        private readonly PassengerClient _passengerClient;
        private readonly IPublishEndpoint _publish;
        private readonly ILogger<RegistrationService> _logger;
        private readonly IDistributedCache _cache;

        public RegistrationService(ApplicationDbContext db, PassengerClient passengerClient,
            IPublishEndpoint publish, ILogger<RegistrationService> logger, IDistributedCache cache)
        {
            _db = db;
            _passengerClient = passengerClient;
            _publish = publish;
            _logger = logger;
            _cache = cache;
        }

        public async Task<RegistrationAuthResponseDto> AuthenticateAsync(RegistrationAuthRequestDto req)
        {
            var dynamicId = Guid.NewGuid().ToString();
            _logger.LogInformation("Генерация нового DynamicId: {DynamicId}", dynamicId);

            await _publish.Publish(new DynamicIdRegistered
            {
                DynamicId = dynamicId,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation("DynamicId опубликован в шину: {DynamicId}", dynamicId);
            return new() { DynamicId = dynamicId };
        }

        public async Task<RegistrationOrderSearchResponseDto> SearchOrderAsync(RegistrationOrderSearchRequestDto req)
        {
            _logger.LogInformation("Поиск заказа: PNR={PnrId}, LastName={LastName}", req.PnrId, req.LastName);

            var order = await _passengerClient.GetOrderByPnrAndLastnameAsync(req.DynamicId, req.PnrId, req.LastName);

            if (order == null)
            {
                _logger.LogWarning("Заказ не найден в PassengerService: PNR={PnrId}, LastName={LastName}", req.PnrId,
                    req.LastName);
                throw new KeyNotFoundException("Order not found in Passenger Service");
            }

            _logger.LogInformation("Заказ найден: OrderId={OrderId}", order.OrderId);
            return new RegistrationOrderSearchResponseDto { Order = order };
        }

        public async Task<RegistrationSeatReserveResponseDto> ReserveSeatAsync(RegistrationSeatReserveRequestDto req)
        {
            _logger.LogInformation(
                "Попытка резервирования места: PassengerId={PassengerId}, DepartureId={DepartureId}, Seat={SeatNumber}",
                req.PassengerId, req.DepartureId, req.SeatNumber);

            var existing = await _db.SeatReservations.FirstOrDefaultAsync(s =>
                s.DepartureId == req.DepartureId &&
                s.SeatNumber == req.SeatNumber &&
                s.PassengerId != req.PassengerId
            );

            if (existing != null)
            {
                _logger.LogWarning("Место уже занято другим пассажиром: Seat={SeatNumber}, DepartureId={DepartureId}",
                    req.SeatNumber, req.DepartureId);
                throw new InvalidOperationException(
                    $"Seat {req.SeatNumber} на рейсе {req.DepartureId} уже занято другим пассажиром.");
            }

            var ownExisting = await _db.SeatReservations.FirstOrDefaultAsync(s =>
                s.DepartureId == req.DepartureId &&
                s.SeatNumber == req.SeatNumber &&
                s.PassengerId == req.PassengerId
            );

            if (ownExisting != null)
            {
                _logger.LogInformation("Место уже зарезервировано текущим пассажиром: Seat={SeatNumber}",
                    ownExisting.SeatNumber);
                return new RegistrationSeatReserveResponseDto
                {
                    Seat = new() { SeatNumber = ownExisting.SeatNumber }
                };
            }

            var res = new SeatReservation
            {
                DynamicId = req.DynamicId,
                DepartureId = req.DepartureId,
                PassengerId = req.PassengerId,
                SeatNumber = req.SeatNumber,
                ReservedAt = DateTime.UtcNow
            };

            _db.SeatReservations.Add(res);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Место успешно зарезервировано: Seat={SeatNumber}", res.SeatNumber);

            return new RegistrationSeatReserveResponseDto
            {
                Seat = new() { SeatNumber = res.SeatNumber }
            };
        }

        public async Task<RegistrationPassengerResponseDto> RegisterFreeAsync(RegistrationPassengerFreeRequestDto req)
        {
            _logger.LogInformation("Бесплатная регистрация пассажира: PassengerId={PassengerId}", req.PassengerId);
            return await RegisterAsync(req.DynamicId, req.DepartureId, req.PassengerId, req.SeatNumber, false);
        }

        public async Task<RegistrationPassengerResponseDto> RegisterPaidAsync(RegistrationPassengerPaidRequestDto req)
        {
            _logger.LogInformation("Платная регистрация пассажира: PassengerId={PassengerId}", req.PassengerId);
            return await RegisterAsync(req.DynamicId, req.DepartureId, req.PassengerId, req.SeatNumber, true);
        }
        //Добавить исключение на то, что пассажир не зареган
        private async Task<RegistrationPassengerResponseDto> RegisterAsync(
            string dynamicId, string departureId, string passengerId, string seatNumber, bool paid)
        {
            _logger.LogInformation("Регистрация пассажира: PassengerId={PassengerId}, Paid={Paid}", passengerId, paid);

            var passenger = await _passengerClient.GetPassengerByIdAsync(dynamicId, passengerId);
            
            if (passenger == null)
            {
                _logger.LogWarning("Пассажир не найден в PassengerService: PassengerId={PassengerId}", passengerId);
                throw new KeyNotFoundException("Passenger not found in Passenger Service");
            }

            if (paid)
            {
                var payment = await _db.Payments.FirstOrDefaultAsync(p =>
                    p.DynamicId == dynamicId &&
                    p.PassengerId == passengerId &&
                    p.DepartureId == departureId &&
                    p.Amount > 0 &&
                    p.IsPaid);

                if (payment == null)
                {
                    _logger.LogError("Оплата не найдена: PassengerId={PassengerId}, DepartureId={DepartureId}",
                        passengerId, departureId);
                    throw new Exception($"Оплата не найдена для пассажира {passengerId} на рейсе {departureId}.");
                }

                if (payment.Amount <= 0)
                {
                    _logger.LogError("Сумма оплаты недопустима: {Amount}", payment.Amount);
                    throw new Exception($"Оплата найдена, но сумма {payment.Amount} недопустима.");
                }
            }

            var rec = new RegistrationRecord
            {
                DynamicId = dynamicId,
                DepartureId = departureId,
                PassengerId = passengerId,
                SeatNumber = seatNumber,
                IsPaid = paid,
                RegisteredAt = DateTime.UtcNow
            };

            _db.RegistrationRecords.Add(rec);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Пассажир успешно зарегистрирован: PassengerId={PassengerId}, Seat={Seat}, Paid={Paid}", passengerId,
                seatNumber, paid);

            return new RegistrationPassengerResponseDto
            {
                DynamicId = dynamicId,
                DepartureId = departureId,
                PassengerId = passengerId,
                SeatNumber = seatNumber,
                IsPaid = paid,
                RegisteredAt = rec.RegisteredAt
            };
        }

        public async Task<bool> SimulatePaymentAsync(string dynamicId, string passengerId, string departureId,
            decimal amount)
        {
            _logger.LogInformation("Симуляция оплаты: PassengerId={PassengerId}, Amount={Amount}", passengerId, amount);

            var payment = new Payment
            {
                DynamicId = dynamicId,
                PassengerId = passengerId,
                DepartureId = departureId,
                Amount = amount,
                IsPaid = true,
                PaidAt = DateTime.UtcNow
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Оплата успешно сохранена в БД: PassengerId={PassengerId}, Amount={Amount}",
                passengerId, amount);
            return true;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            var cached = await _cache.GetStringAsync("admin:payments");
            if (cached != null)
            {
                _logger.LogInformation("ADMIN (RegistrationService): Получены платежи из кэша");
                return JsonSerializer.Deserialize<IEnumerable<Payment>>(cached)!;
            }

            await Task.Delay(300);
            
            var data = await _db.Payments.ToListAsync();
            await _cache.SetStringAsync("admin:payments", JsonSerializer.Serialize(data), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            return data;
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            string cacheKey = $"admin:payments:{id}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("ADMIN (RegistrationService): Платёж {Id} получен из кэша", id);
                return JsonSerializer.Deserialize<Payment>(cached);
            }
            
            await Task.Delay(300);

            _logger.LogInformation("ADMIN (RegistrationService): Запрос платежа {Id} из БД", id);
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment != null)
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(payment),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });

                _logger.LogInformation("ADMIN (RegistrationService): Платёж {Id} закэширован", id);
            }

            return payment;
        }

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            _logger.LogInformation("ADMIN (RegistrationService): Создание нового платежа для пассажира {PassengerId}",
                payment.PassengerId);
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("admin:payments");
            return payment;
        }

        public async Task<Payment?> UpdatePaymentAsync(int id, Payment payment)
        {
            var entity = await _db.Payments.FindAsync(id);
            if (entity == null) return null;
            _db.Entry(entity).CurrentValues.SetValues(payment);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("admin:payments");
            _logger.LogInformation("ADMIN (RegistrationService): Обновлен платёж с ID {PaymentId}", id);
            return entity;
        }

        public async Task<bool> DeletePaymentAsync(int id)
        {
            var entity = await _db.Payments.FindAsync(id);
            if (entity == null) return false;
            _db.Payments.Remove(entity);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("admin:payments");
            _logger.LogWarning("ADMIN (RegistrationService): Удалён платёж с ID {PaymentId}", id);
            return true;
        }

        public async Task<IEnumerable<RegistrationRecord>> GetAllRegistrationsAsync()
        {
            const string cacheKey = "admin:registrations";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("ADMIN (RegistrationService): Получены регистрации из кэша");
                return JsonSerializer.Deserialize<IEnumerable<RegistrationRecord>>(cached)!;
            }
            
            await Task.Delay(300);

            var data = await _db.RegistrationRecords.ToListAsync();
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(data), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            return data;
        }

        public async Task<RegistrationRecord?> GetRegistrationByIdAsync(int id)
        {
            string cacheKey = $"admin:registrations:{id}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("ADMIN (RegistrationService): Регистрация {Id} получена из кэша", id);
                return JsonSerializer.Deserialize<RegistrationRecord>(cached);
            }
            
            await Task.Delay(300);

            _logger.LogInformation("ADMIN (RegistrationService): Запрос регистрации {Id} из БД", id);
            var registration = await _db.RegistrationRecords.FirstOrDefaultAsync(r => r.RegistrationRecordId == id);

            if (registration != null)
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(registration),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });

                _logger.LogInformation("ADMIN (RegistrationService): Регистрация {Id} закэширована", id);
            }

            return registration;
        }

        public async Task<RegistrationRecord> CreateRegistrationAsync(RegistrationRecord record)
        {
            _logger.LogInformation(
                "ADMIN (RegistrationService): Создание новой регистрации для пассажира {PassengerId}",
                record.PassengerId);
            _db.RegistrationRecords.Add(record);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("admin:registrations");
            return record;
        }

        public async Task<RegistrationRecord?> UpdateRegistrationAsync(int id, RegistrationRecord updated)
        {
            var entity = await _db.RegistrationRecords.FindAsync(id);
            if (entity == null) return null;
            _db.Entry(entity).CurrentValues.SetValues(updated);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("admin:registrations");
            _logger.LogInformation("ADMIN (RegistrationService): Обновлена регистрация с ID {RegistrationId}", id);
            return entity;
        }

        public async Task<bool> DeleteRegistrationAsync(int id)
        {
            var entity = await _db.RegistrationRecords.FindAsync(id);
            if (entity == null) return false;
            _db.RegistrationRecords.Remove(entity);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("admin:registrations");
            _logger.LogWarning("ADMIN (RegistrationService): Удалена регистрация с ID {RegistrationId}", id);
            return true;
        }

        public async Task<IEnumerable<SeatReservation>> GetAllReservationsAsync()
        {
            const string cacheKey = "admin:reservations";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("ADMIN (RegistrationService): Получены резервации из кэша");
                return JsonSerializer.Deserialize<IEnumerable<SeatReservation>>(cached)!;
            }
            
            await Task.Delay(300);

            var data = await _db.SeatReservations.ToListAsync();
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(data), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            return data;
        }

        public async Task<SeatReservation?> GetReservationByIdAsync(int id)
        {
            string cacheKey = $"admin:reservations:{id}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("ADMIN (RegistrationService): Резервация {Id} получена из кэша", id);
                return JsonSerializer.Deserialize<SeatReservation>(cached);
            }
            
            await Task.Delay(300);

            _logger.LogInformation("ADMIN (RegistrationService): Запрос резервации {Id} из БД", id);
            var reservation = await _db.SeatReservations.FirstOrDefaultAsync(r => r.SeatReservationId == id);

            if (reservation != null)
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(reservation),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });

                _logger.LogInformation("ADMIN (RegistrationService): Резервация {Id} закэширована", id);
            }

            return reservation;
        }

        public async Task<SeatReservation> CreateReservationAsync(SeatReservation reservation)
        {
            _logger.LogInformation(
                "ADMIN (RegistrationService): Создание резервации для пассажира {PassengerId} на место {SeatNumber}",
                reservation.PassengerId, reservation.SeatNumber);
            _db.SeatReservations.Add(reservation);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("admin:reservations");
            return reservation;
        }

        public async Task<SeatReservation?> UpdateReservationAsync(int id, SeatReservation updated)
        {
            var entity = await _db.SeatReservations.FindAsync(id);
            if (entity == null) return null;
            _db.Entry(entity).CurrentValues.SetValues(updated);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("admin:reservations");
            _logger.LogInformation("ADMIN (RegistrationService): Обновлена резервация с ID {ReservationId}", id);
            return entity;
        }

        public async Task<bool> DeleteReservationAsync(int id)
        {
            var entity = await _db.SeatReservations.FindAsync(id);
            if (entity == null) return false;
            _db.SeatReservations.Remove(entity);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync("admin:reservations");
            _logger.LogWarning("ADMIN (RegistrationService): Удалена резервация с ID {ReservationId}", id);
            return true;
        }
    }
}