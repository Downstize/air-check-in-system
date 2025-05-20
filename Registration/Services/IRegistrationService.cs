using Registration.DTO.Auth;
using Registration.DTO.Order;
using Registration.DTO.Registration;
using Registration.Models;

namespace Registration.Services
{
    public interface IRegistrationService
    {
        Task<RegistrationAuthResponseDto> AuthenticateAsync(RegistrationAuthRequestDto req);
        Task<RegistrationOrderSearchResponseDto> SearchOrderAsync(RegistrationOrderSearchRequestDto req);
        Task<RegistrationSeatReserveResponseDto> ReserveSeatAsync(RegistrationSeatReserveRequestDto req);
        Task<RegistrationPassengerResponseDto> RegisterFreeAsync(RegistrationPassengerFreeRequestDto req);
        Task<RegistrationPassengerResponseDto> RegisterPaidAsync(RegistrationPassengerPaidRequestDto req);
        Task<bool> SimulatePaymentAsync(string dynamicId, string passengerId, string departureId, decimal amount);
        
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task<Payment?> UpdatePaymentAsync(int id, Payment payment);
        Task<bool> DeletePaymentAsync(int id);
        
        Task<IEnumerable<RegistrationRecord>> GetAllRegistrationsAsync();
        Task<RegistrationRecord?> GetRegistrationByIdAsync(int id);
        Task<RegistrationRecord> CreateRegistrationAsync(RegistrationRecord record);
        Task<RegistrationRecord?> UpdateRegistrationAsync(int id, RegistrationRecord record);
        Task<bool> DeleteRegistrationAsync(int id);

        Task<IEnumerable<SeatReservation>> GetAllReservationsAsync();
        Task<SeatReservation?> GetReservationByIdAsync(int id);
        Task<SeatReservation> CreateReservationAsync(SeatReservation reservation);
        Task<SeatReservation?> UpdateReservationAsync(int id, SeatReservation reservation);
        Task<bool> DeleteReservationAsync(int id);
    }
}