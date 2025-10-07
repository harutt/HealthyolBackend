using HealthyolBackend.DTOs;
using HealthyolBackend.Models;

namespace HealthyolBackend.Services
{
    public interface IHospitalService
    {
        Task<IEnumerable<HospitalDto>> GetAllAsync();
        Task<HospitalDto?> GetByIdAsync(Guid id);
        Task<HospitalDto> CreateAsync(CreateHospitalDto createDto);
        Task<HospitalDto?> UpdateAsync(Guid id, UpdateHospitalDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<(IEnumerable<HospitalDto> Hospitals, int TotalCount)> SearchAsync(HospitalSearchDto searchDto);
        Task<bool> AddServiceAsync(Guid hospitalId, Guid serviceId, string? description = null, decimal? price = null);
        Task<bool> RemoveServiceAsync(Guid hospitalId, Guid serviceId);
        Task<IEnumerable<HealthServiceDto>> GetServicesAsync(Guid hospitalId);
    }
}