using HealthyolBackend.Data;
using HealthyolBackend.DTOs;
using HealthyolBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthyolBackend.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly ApplicationDbContext _context;

        public DoctorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DoctorDto>> GetAllAsync()
        {
            var doctors = await _context.Doctors
                .Include(d => d.Hospital)
                .Include(d => d.DoctorHealthService)
                    .ThenInclude(dhs => dhs.HealthService)
                .Where(d => d.IsActive)
                .OrderBy(d => d.FullName)
                .ToListAsync();

            return doctors.Select(MapToDto);
        }

        public async Task<DoctorDto?> GetByIdAsync(Guid id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Hospital)
                .Include(d => d.DoctorHealthService)
                    .ThenInclude(dhs => dhs.HealthService)
                .FirstOrDefaultAsync(d => d.Id == id);

            return doctor != null ? MapToDto(doctor) : null;
        }

        public async Task<DoctorDto> CreateAsync(CreateDoctorDto createDto)
        {
            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                FullName = createDto.FullName,
                Specialty = createDto.Specialty,
                ExperienceYears = createDto.ExperienceYears,
                LanguagesSpoken = createDto.LanguagesSpoken,
                Phone = createDto.Phone,
                Email = createDto.Email,
                Website = createDto.Website,
                Description = createDto.Description,
                ProfileImageUrl = createDto.ProfileImageUrl,
                HospitalId = createDto.HospitalId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Add services if specified
            if (createDto.ServiceIds.Any())
            {
                var doctorServices = createDto.ServiceIds.Select(serviceId => new DoctorHealthService
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctor.Id,
                    HealthServiceId = serviceId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.DoctorServices.AddRange(doctorServices);
                await _context.SaveChangesAsync();
            }

            return await GetByIdAsync(doctor.Id) ?? throw new InvalidOperationException("Failed to retrieve created doctor");
        }

        public async Task<DoctorDto?> UpdateAsync(Guid id, UpdateDoctorDto updateDto)
        {
            var doctor = await _context.Doctors
                .Include(d => d.DoctorHealthService)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return null;

            doctor.FullName = updateDto.FullName;
            doctor.Specialty = updateDto.Specialty;
            doctor.ExperienceYears = updateDto.ExperienceYears;
            doctor.LanguagesSpoken = updateDto.LanguagesSpoken;
            doctor.Phone = updateDto.Phone;
            doctor.Email = updateDto.Email;
            doctor.Website = updateDto.Website;
            doctor.Description = updateDto.Description;
            doctor.ProfileImageUrl = updateDto.ProfileImageUrl;
            doctor.HospitalId = updateDto.HospitalId;
            doctor.IsActive = updateDto.IsActive;

            // Update services
            _context.DoctorServices.RemoveRange(doctor.DoctorHealthService);
            
            if (updateDto.ServiceIds.Any())
            {
                var doctorServices = updateDto.ServiceIds.Select(serviceId => new DoctorHealthService
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctor.Id,
                    HealthServiceId = serviceId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.DoctorServices.AddRange(doctorServices);
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(doctor.Id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return false;

            // Soft delete - just mark as inactive
            doctor.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<DoctorDto>> GetByHospitalIdAsync(Guid hospitalId)
        {
            var doctors = await _context.Doctors
                .Include(d => d.Hospital)
                .Include(d => d.DoctorHealthService)
                    .ThenInclude(dhs => dhs.HealthService)
                .Where(d => d.HospitalId == hospitalId && d.IsActive)
                .OrderBy(d => d.FullName)
                .ToListAsync();

            return doctors.Select(MapToDto);
        }

        public async Task<IEnumerable<DoctorDto>> SearchAsync(string query)
        {
            var doctors = await _context.Doctors
                .Include(d => d.Hospital)
                .Include(d => d.DoctorHealthService)
                    .ThenInclude(dhs => dhs.HealthService)
                .Where(d => d.IsActive && 
                    (d.FullName.Contains(query) || 
                     d.Specialty!.Contains(query) ||
                     d.Description!.Contains(query)))
                .OrderBy(d => d.FullName)
                .ToListAsync();

            return doctors.Select(MapToDto);
        }

        private static DoctorDto MapToDto(Doctor doctor)
        {
            return new DoctorDto
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                Specialty = doctor.Specialty,
                ExperienceYears = doctor.ExperienceYears,
                LanguagesSpoken = doctor.LanguagesSpoken,
                Phone = doctor.Phone,
                Email = doctor.Email,
                Website = doctor.Website,
                Description = doctor.Description,
                ProfileImageUrl = doctor.ProfileImageUrl,
                AverageRating = doctor.AverageRating,
                ReviewCount = doctor.ReviewCount,
                IsActive = doctor.IsActive,
                CreatedAt = doctor.CreatedAt,
                HospitalId = doctor.HospitalId,
                HospitalName = doctor.Hospital?.Name,
                Services = doctor.DoctorHealthService.Select(dhs => new HealthServiceDto
                {
                    Id = dhs.HealthService.Id,
                    Name = dhs.HealthService.Name,
                    Description = dhs.HealthService.Description,
                    IconKey = dhs.HealthService.IconKey,
                    IconUrl = dhs.HealthService.IconUrl,
                    ImageUrl = dhs.HealthService.ImageUrl,
                    Category = dhs.HealthService.Category,
                    IsActive = dhs.HealthService.IsActive,
                    CreatedAt = dhs.HealthService.CreatedAt
                }).ToList()
            };
        }
    }
}