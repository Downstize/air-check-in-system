using Baggage.DTO;
using Shared.Contracts;

namespace Baggage.Services
{
    public interface IBaggageService
    {
        Task<BaggageAllowanceDto>  GetAllowanceAsync(string dynamicId, string orderId, string passengerId);
        Task<BaggageRegistrationDto> RegisterAsync (string dynamicId, string orderId, string passengerId, int pieces, decimal weightKg);
        Task<bool> CancelAsync(string dynamicId, string orderId, string passengerId);
        Task<bool> SimulateBaggagePaymentAsync(string dynamicId, string passengerId, string departureId, decimal amount);
    }
}