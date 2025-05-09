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

        public RegistrationService(ApplicationDbContext db, PassengerClient passengerClient, IPublishEndpoint publish)
        {
            _db = db;
            _passengerClient = passengerClient;
            _publish = publish;
        }

        public async Task<RegistrationAuthResponseDto> AuthenticateAsync(RegistrationAuthRequestDto req)
        {
            var dynamicId = Guid.NewGuid().ToString();
            
            await _publish.Publish(new DynamicIdRegistered
            {
                DynamicId = dynamicId,
                CreatedAt = DateTime.UtcNow
            });

            return new() { DynamicId = dynamicId };
        }

        public async Task<RegistrationOrderSearchResponseDto> SearchOrderAsync(RegistrationOrderSearchRequestDto req)
        {
            var order = await _passengerClient.GetOrderByPnrAndLastnameAsync(req.DynamicId, req.PnrId, req.LastName);

            if (order == null)
                throw new KeyNotFoundException("Order not found in Passenger Service");

            return new RegistrationOrderSearchResponseDto { Order = order };
        }

        public async Task<RegistrationSeatReserveResponseDto> ReserveSeatAsync(RegistrationSeatReserveRequestDto req)
        {
            var existing = await _db.SeatReservations.FirstOrDefaultAsync(s =>
                    s.DepartureId == req.DepartureId &&
                    s.SeatNumber == req.SeatNumber &&
                    s.PassengerId != req.PassengerId
            );

            if (existing != null)
            {
                throw new InvalidOperationException($"Seat {req.SeatNumber} на рейсе {req.DepartureId} уже занято другим пассажиром.");
            }
            
            var ownExisting = await _db.SeatReservations.FirstOrDefaultAsync(s =>
                s.DepartureId == req.DepartureId &&
                s.SeatNumber == req.SeatNumber &&
                s.PassengerId == req.PassengerId
            );

            if (ownExisting != null)
            {
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

            return new RegistrationSeatReserveResponseDto
            {
                Seat = new() { SeatNumber = res.SeatNumber }
            };
        }


        public async Task<RegistrationPassengerResponseDto> RegisterFreeAsync(RegistrationPassengerFreeRequestDto req)
            => await RegisterAsync(req.DynamicId, req.DepartureId, req.PassengerId, req.SeatNumber, false);

        public async Task<RegistrationPassengerResponseDto> RegisterPaidAsync(RegistrationPassengerPaidRequestDto req)
            => await RegisterAsync(req.DynamicId, req.DepartureId, req.PassengerId, req.SeatNumber, true);

        private async Task<RegistrationPassengerResponseDto> RegisterAsync(
            string dynamicId, string departureId, string passengerId, string seatNumber, bool paid)
        {
            var passenger = await _passengerClient.GetPassengerByIdAsync(dynamicId,passengerId);
            if (passenger == null)
                throw new KeyNotFoundException("Passenger not found in Passenger Service");
            
            if (paid)
            {
                var payment = await _db.Payments.FirstOrDefaultAsync(p =>
                    p.DynamicId == dynamicId &&
                    p.PassengerId == passengerId &&
                    p.DepartureId == departureId &&
                    p.Amount > 0 &&
                    p.IsPaid);

                if (payment == null)
                    throw new Exception($"Оплата не найдена для пассажира {passengerId} на рейсе {departureId}.");
                
                if (payment.Amount <= 0)
                    throw new Exception($"Оплата найдена, но сумма {payment.Amount} недопустима.");
            }

            var rec = new RegistrationRecord {
                DynamicId      = dynamicId,
                DepartureId    = departureId,
                PassengerId    = passengerId,
                SeatNumber     = seatNumber,
                IsPaid         = paid,
                RegisteredAt   = DateTime.UtcNow
            };
            _db.RegistrationRecords.Add(rec);
            await _db.SaveChangesAsync();
            return new RegistrationPassengerResponseDto {
                DynamicId    = dynamicId,
                DepartureId  = departureId,
                PassengerId  = passengerId,
                SeatNumber   = seatNumber,
                IsPaid       = paid,
                RegisteredAt = rec.RegisteredAt
            };
        }
        
        public async Task<bool> SimulatePaymentAsync(string dynamicId, string passengerId, string departureId, decimal amount)
        {
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
            return true;
        }

    }
}
