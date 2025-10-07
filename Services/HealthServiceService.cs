using HealthyolBackend.Data;
using HealthyolBackend.DTOs;
using HealthyolBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthyolBackend.Services
{
    public class HealthServiceService : IHealthServiceService
    {
        private readonly ApplicationDbContext _context;

        public HealthServiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HealthServiceDto>> GetAllAsync()
        {
            var services = await _context.HealthServices
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return services.Select(MapToDto);
        }

        public async Task<HealthServiceDto?> GetByIdAsync(Guid id)
        {
            var service = await _context.HealthServices
                .FirstOrDefaultAsync(s => s.Id == id);

            return service != null ? MapToDto(service) : null;
        }

        public async Task<HealthServiceDto> CreateAsync(CreateHealthServiceDto createDto)
        {
            var service = new HealthService
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Description = createDto.Description,
                IconKey = createDto.IconKey,
                IconUrl = createDto.IconUrl,
                ImageUrl = createDto.ImageUrl,
                Category = createDto.Category,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.HealthServices.Add(service);
            await _context.SaveChangesAsync();

            return MapToDto(service);
        }

        public async Task<HealthServiceDto?> UpdateAsync(Guid id, UpdateHealthServiceDto updateDto)
        {
            var service = await _context.HealthServices
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
                return null;

            service.Name = updateDto.Name;
            service.Description = updateDto.Description;
            service.IconKey = updateDto.IconKey;
            service.IconUrl = updateDto.IconUrl;
            service.ImageUrl = updateDto.ImageUrl;
            service.Category = updateDto.Category;
            service.IsActive = updateDto.IsActive;

            await _context.SaveChangesAsync();

            return MapToDto(service);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var service = await _context.HealthServices
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
                return false;

            // Soft delete - just mark as inactive
            service.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<HealthServiceDto>> GetByCategoryAsync(string category)
        {
            var services = await _context.HealthServices
                .Where(s => s.IsActive && s.Category == category)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return services.Select(MapToDto);
        }

        private static HealthServiceDto MapToDto(HealthService service)
        {
            return new HealthServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                IconKey = service.IconKey,
                IconUrl = service.IconUrl,
                ImageUrl = service.ImageUrl,
                Category = service.Category,
                IsActive = service.IsActive,
                CreatedAt = service.CreatedAt
            };
        }
    }
}