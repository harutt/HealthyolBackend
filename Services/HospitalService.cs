using Microsoft.EntityFrameworkCore;
using HealthyolBackend.Data;
using HealthyolBackend.DTOs;
using HealthyolBackend.Models;

namespace HealthyolBackend.Services
{
    public class HospitalService : IHospitalService
    {
        private readonly ApplicationDbContext _context;

        public HospitalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HospitalDto>> GetAllAsync()
        {
            var hospitals = await _context.Hospitals
                .Include(h => h.HospitalHealthService)
                    .ThenInclude(hs => hs.HealthService)
                .Include(h => h.Doctors)
                .Where(h => h.IsActive)
                .ToListAsync();

            return hospitals.Select(MapToDto);
        }

        public async Task<HospitalDto?> GetByIdAsync(Guid id)
        {
            var hospital = await _context.Hospitals
                .Include(h => h.HospitalHealthService)
                    .ThenInclude(hs => hs.HealthService)
                .Include(h => h.Doctors)
                .FirstOrDefaultAsync(h => h.Id == id);

            return hospital == null ? null : MapToDto(hospital);
        }

        public async Task<HospitalDto> CreateAsync(CreateHospitalDto createDto)
        {
            var hospital = new Hospital
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Email = createDto.Email,
                Address = createDto.Address,
                City = createDto.City,
                Country = createDto.Country,
                GoogleMapsUrl = createDto.GoogleMapsUrl,
                GooglePlacesUrl = createDto.GooglePlacesUrl,
                Phone = createDto.Phone,
                Website = createDto.Website,
                Description = createDto.Description,
                LogoUrl = createDto.LogoUrl,
                ImageUrls = createDto.ImageUrls,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Hospitals.Add(hospital);

            // Add services
            foreach (var serviceId in createDto.ServiceIds)
            {
                var hospitalService = new HospitalHealthService
                {
                    Id = Guid.NewGuid(),
                    HospitalId = hospital.Id,
                    HealthServiceId = serviceId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.HospitalServices.Add(hospitalService);
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(hospital.Id) ?? throw new InvalidOperationException("Failed to create hospital");
        }

        public async Task<HospitalDto?> UpdateAsync(Guid id, UpdateHospitalDto updateDto)
        {
            var hospital = await _context.Hospitals.FindAsync(id);
            if (hospital == null) return null;

            hospital.Name = updateDto.Name;
            hospital.Email = updateDto.Email;
            hospital.Address = updateDto.Address;
            hospital.City = updateDto.City;
            hospital.Country = updateDto.Country;
            hospital.GoogleMapsUrl = updateDto.GoogleMapsUrl;
            hospital.GooglePlacesUrl = updateDto.GooglePlacesUrl;
            hospital.Phone = updateDto.Phone;
            hospital.Website = updateDto.Website;
            hospital.Description = updateDto.Description;
            hospital.LogoUrl = updateDto.LogoUrl;
            hospital.ImageUrls = updateDto.ImageUrls;
            hospital.IsActive = updateDto.IsActive;
            hospital.UpdatedAt = DateTime.UtcNow;

            // Update services
            var existingServices = await _context.HospitalServices
                .Where(hs => hs.HospitalId == id)
                .ToListAsync();

            _context.HospitalServices.RemoveRange(existingServices);

            foreach (var serviceId in updateDto.ServiceIds)
            {
                var hospitalService = new HospitalHealthService
                {
                    Id = Guid.NewGuid(),
                    HospitalId = id,
                    HealthServiceId = serviceId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.HospitalServices.Add(hospitalService);
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var hospital = await _context.Hospitals.FindAsync(id);
            if (hospital == null) return false;

            hospital.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<HospitalDto> Hospitals, int TotalCount)> SearchAsync(HospitalSearchDto searchDto)
        {
            var query = _context.Hospitals
                .Include(h => h.HospitalHealthService)
                    .ThenInclude(hs => hs.HealthService)
                .Include(h => h.Doctors)
                .Where(h => h.IsActive);

            // Apply filters
            if (!string.IsNullOrEmpty(searchDto.Query))
            {
                query = query.Where(h =>
                    h.Name.Contains(searchDto.Query) ||
                    h.Description!.Contains(searchDto.Query) ||
                    h.City!.Contains(searchDto.Query));
            }

            if (!string.IsNullOrEmpty(searchDto.City))
            {
                query = query.Where(h => h.City == searchDto.City);
            }

            if (!string.IsNullOrEmpty(searchDto.Country))
            {
                query = query.Where(h => h.Country == searchDto.Country);
            }

            if (searchDto.ServiceIds.Any())
            {
                query = query.Where(h => h.HospitalHealthService.Any(hs => searchDto.ServiceIds.Contains(hs.HealthServiceId)));
            }

            if (searchDto.MinRating.HasValue)
            {
                query = query.Where(h => h.AverageRating >= searchDto.MinRating.Value);
            }

            if (searchDto.IsVerified.HasValue)
            {
                query = query.Where(h => h.IsVerified == searchDto.IsVerified.Value);
            }

            // Apply sorting
            query = searchDto.SortBy.ToLower() switch
            {
                "rating" => searchDto.SortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(h => h.AverageRating)
                    : query.OrderBy(h => h.AverageRating),
                "created" => searchDto.SortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(h => h.CreatedAt)
                    : query.OrderBy(h => h.CreatedAt),
                _ => searchDto.SortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(h => h.Name)
                    : query.OrderBy(h => h.Name)
            };

            var totalCount = await query.CountAsync();

            var hospitals = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return (hospitals.Select(MapToDto), totalCount);
        }

        public async Task<bool> AddServiceAsync(Guid hospitalId, Guid serviceId, string? description = null, decimal? price = null)
        {
            var exists = await _context.HospitalServices
                .AnyAsync(hs => hs.HospitalId == hospitalId && hs.HealthServiceId == serviceId);

            if (exists) return false;

            var hospitalService = new HospitalHealthService
            {
                Id = Guid.NewGuid(),
                HospitalId = hospitalId,
                HealthServiceId = serviceId,
                Description = description,
                Price = price,
                CreatedAt = DateTime.UtcNow
            };

            _context.HospitalServices.Add(hospitalService);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveServiceAsync(Guid hospitalId, Guid serviceId)
        {
            var hospitalService = await _context.HospitalServices
                .FirstOrDefaultAsync(hs => hs.HospitalId == hospitalId && hs.HealthServiceId == serviceId);

            if (hospitalService == null) return false;

            _context.HospitalServices.Remove(hospitalService);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<HealthServiceDto>> GetServicesAsync(Guid hospitalId)
        {
            return await _context.HospitalServices
                .Where(hs => hs.HospitalId == hospitalId)
                .Include(hs => hs.HealthService)
                .OrderBy(hs => hs.HealthService.Name)
                .Select(hs => new HealthServiceDto
                {
                    Id = hs.HealthService.Id,
                    Name = hs.HealthService.Name,
                    Description = hs.HealthService.Description,
                    Category = hs.HealthService.Category,
                    IconKey = hs.HealthService.IconKey,
                    IconUrl = hs.HealthService.IconUrl,
                    ImageUrl = hs.HealthService.ImageUrl,
                    IsActive = hs.HealthService.IsActive,
                    CreatedAt = hs.HealthService.CreatedAt
                })
                .ToListAsync();
        }

        private static HospitalDto MapToDto(Hospital hospital)
        {
            return new HospitalDto
            {
                Id = hospital.Id,
                Name = hospital.Name,
                Email = hospital.Email,
                Address = hospital.Address,
                City = hospital.City,
                Country = hospital.Country,
                GoogleMapsUrl = hospital.GoogleMapsUrl,
                GooglePlacesUrl = hospital.GooglePlacesUrl,
                Phone = hospital.Phone,
                Website = hospital.Website,
                Description = hospital.Description,
                LogoUrl = hospital.LogoUrl,
                ImageUrls = hospital.ImageUrls,
                AverageRating = hospital.AverageRating,
                ReviewCount = hospital.ReviewCount,
                IsActive = hospital.IsActive,
                IsVerified = hospital.IsVerified,
                CreatedAt = hospital.CreatedAt,
                UpdatedAt = hospital.UpdatedAt,
                Services = hospital.HospitalHealthService.Select(hs => new HealthServiceDto
                {
                    Id = hs.HealthService.Id,
                    Name = hs.HealthService.Name,
                    Description = hs.HealthService.Description,
                    IconKey = hs.HealthService.IconKey,
                    IconUrl = hs.HealthService.IconUrl,
                    ImageUrl = hs.HealthService.ImageUrl,
                    Category = hs.HealthService.Category,
                    IsActive = hs.HealthService.IsActive,
                    CreatedAt = hs.HealthService.CreatedAt
                }).ToList(),
                Doctors = hospital.Doctors.Select(d => new DoctorDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    Specialty = d.Specialty,
                    ExperienceYears = d.ExperienceYears,
                    LanguagesSpoken = d.LanguagesSpoken,
                    Phone = d.Phone,
                    Email = d.Email,
                    Website = d.Website,
                    Description = d.Description,
                    ProfileImageUrl = d.ProfileImageUrl,
                    AverageRating = d.AverageRating,
                    ReviewCount = d.ReviewCount,
                    IsActive = d.IsActive,
                    CreatedAt = d.CreatedAt,
                    HospitalId = d.HospitalId,
                    HospitalName = hospital.Name
                }).ToList()
            };
        }
    }
}