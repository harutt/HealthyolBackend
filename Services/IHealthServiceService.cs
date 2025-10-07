using HealthyolBackend.DTOs;

namespace HealthyolBackend.Services
{
    public interface IHealthServiceService
    {
        Task<IEnumerable<HealthServiceDto>> GetAllAsync();
        Task<HealthServiceDto?> GetByIdAsync(Guid id);
        Task<HealthServiceDto> CreateAsync(CreateHealthServiceDto createDto);
        Task<HealthServiceDto?> UpdateAsync(Guid id, UpdateHealthServiceDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<HealthServiceDto>> GetByCategoryAsync(string category);
    }
}