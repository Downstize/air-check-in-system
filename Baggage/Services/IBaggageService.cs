using Baggage.DTO;
using Baggage.Models;
using Shared.Contracts;

namespace Baggage.Services
{
    public interface IBaggageService
    {
        Task<BaggageAllowanceDto>  GetAllowanceAsync(string dynamicId, string orderId, string passengerId);
        Task<BaggageRegistrationDto> RegisterAsync (string dynamicId, string orderId, string passengerId, int pieces, decimal weightKg);
        Task<bool> CancelAsync(string dynamicId, string orderId, string passengerId);
        Task<bool> SimulateBaggagePaymentAsync(string dynamicId, string passengerId, string departureId, decimal amount);
        
        Task<BaggagePayment> CreatePaymentAsync(BaggagePayment payment);
        Task<IEnumerable<BaggagePayment>> GetAllPaymentsAsync();
        Task<BaggagePayment?> GetPaymentByIdAsync(string id);
        Task<bool> UpdatePaymentAsync(string id, BaggagePayment updated);
        Task<bool> DeletePaymentAsync(string id);
        
        Task<BaggageRegistration> CreateRegistrationAsync(BaggageRegistration reg);
        Task<IEnumerable<BaggageRegistration>> GetAllRegistrationsAsync();
        Task<BaggageRegistration?> GetRegistrationByIdAsync(string id);
        Task<bool> UpdateRegistrationAsync(string id, BaggageRegistration updated);
        Task<bool> DeleteRegistrationAsync(string id);
        
        Task<PaidOption> CreatePaidOptionAsync(PaidOption option);
        Task<IEnumerable<PaidOption>> GetAllPaidOptionsAsync();
        Task<PaidOption?> GetPaidOptionByIdAsync(string id);
        Task<bool> UpdatePaidOptionAsync(string id, PaidOption updated);
        Task<bool> DeletePaidOptionAsync(string id);
    }
}