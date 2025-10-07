using HealthyolBackend.DTOs;

namespace HealthyolBackend.Services
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorDto>> GetAllAsync();
        Task<DoctorDto?> GetByIdAsync(Guid id);
        Task<DoctorDto> CreateAsync(CreateDoctorDto createDto);
        Task<DoctorDto?> UpdateAsync(Guid id, UpdateDoctorDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<DoctorDto>> GetByHospitalIdAsync(Guid hospitalId);
        Task<IEnumerable<DoctorDto>> SearchAsync(string query);
    }
}