using MassTransit;
using Microsoft.EntityFrameworkCore;
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

        public RegistrationService(ApplicationDbContext db, PassengerClient passengerClient, IPublishEndpoint publish, ILogger<RegistrationService> logger)
        {
            _db = db;
            _passengerClient = passengerClient;
            _publish = publish;
            _logger = logger;
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
                _logger.LogWarning("Заказ не найден в PassengerService: PNR={PnrId}, LastName={LastName}", req.PnrId, req.LastName);
                throw new KeyNotFoundException("Order not found in Passenger Service");
            }

            _logger.LogInformation("Заказ найден: OrderId={OrderId}", order.OrderId);
            return new RegistrationOrderSearchResponseDto { Order = order };
        }

        public async Task<RegistrationSeatReserveResponseDto> ReserveSeatAsync(RegistrationSeatReserveRequestDto req)
        {
            _logger.LogInformation("Попытка резервирования места: PassengerId={PassengerId}, DepartureId={DepartureId}, Seat={SeatNumber}",
                req.PassengerId, req.DepartureId, req.SeatNumber);

            var existing = await _db.SeatReservations.FirstOrDefaultAsync(s =>
                    s.DepartureId == req.DepartureId &&
                    s.SeatNumber == req.SeatNumber &&
                    s.PassengerId != req.PassengerId
            );

            if (existing != null)
            {
                _logger.LogWarning("Место уже занято другим пассажиром: Seat={SeatNumber}, DepartureId={DepartureId}", req.SeatNumber, req.DepartureId);
                throw new InvalidOperationException($"Seat {req.SeatNumber} на рейсе {req.DepartureId} уже занято другим пассажиром.");
            }

            var ownExisting = await _db.SeatReservations.FirstOrDefaultAsync(s =>
                s.DepartureId == req.DepartureId &&
                s.SeatNumber == req.SeatNumber &&
                s.PassengerId == req.PassengerId
            );

            if (ownExisting != null)
            {
                _logger.LogInformation("Место уже зарезервировано текущим пассажиром: Seat={SeatNumber}", ownExisting.SeatNumber);
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
                    _logger.LogError("Оплата не найдена: PassengerId={PassengerId}, DepartureId={DepartureId}", passengerId, departureId);
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

            _logger.LogInformation("Пассажир успешно зарегистрирован: PassengerId={PassengerId}, Seat={Seat}, Paid={Paid}", passengerId, seatNumber, paid);

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

        public async Task<bool> SimulatePaymentAsync(string dynamicId, string passengerId, string departureId, decimal amount)
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

            _logger.LogInformation("Оплата успешно сохранена в БД: PassengerId={PassengerId}, Amount={Amount}", passengerId, amount);
            return true;
        }
    }
}
