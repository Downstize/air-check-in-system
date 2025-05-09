using Registration.DTO.Auth;
using Registration.DTO.Order;
using Registration.DTO.Registration;

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
    }
}