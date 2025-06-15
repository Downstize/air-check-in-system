using Shared.Contracts;

namespace PassengerService.Services;

public interface IPassengersService
{
    Task<IEnumerable<PassengerDto>> GetAllAsync();
    Task<PassengerDto?> GetByIdAsync(string id);
    Task<PassengerDto> CreateAsync(PassengerDto dto);
    Task<bool> UpdateAsync(string id, PassengerDto dto);
    Task<bool> DeleteAsync(string id);
}
