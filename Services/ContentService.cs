using HealthyolBackend.Data;
using HealthyolBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthyolBackend.Services
{
    public class ContentService : IContentService
    {
        private readonly ApplicationDbContext _context;

        public ContentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Content>> GetAllAsync()
        {
            return await _context.Contents
                .Where(c => c.IsActive)
                .OrderBy(c => c.Category)
                .ThenBy(c => c.Key)
                .ToListAsync();
        }

        public async Task<Content?> GetByKeyAsync(string key)
        {
            return await _context.Contents
                .FirstOrDefaultAsync(c => c.Key == key && c.IsActive);
        }

        public async Task<Content> CreateAsync(Content content, string userId)
        {
            content.Id = Guid.NewGuid();
            content.CreatedAt = DateTime.UtcNow;
            content.UpdatedAt = DateTime.UtcNow;
            content.CreatedBy = userId;
            content.UpdatedBy = userId;
            content.IsActive = true;

            _context.Contents.Add(content);
            await _context.SaveChangesAsync();

            return content;
        }

        public async Task<Content?> UpdateAsync(string key, string value, string userId)
        {
            var content = await _context.Contents
                .FirstOrDefaultAsync(c => c.Key == key);

            if (content == null)
                return null;

            content.Value = value;
            content.UpdatedAt = DateTime.UtcNow;
            content.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return content;
        }

        public async Task<bool> DeleteAsync(string key)
        {
            var content = await _context.Contents
                .FirstOrDefaultAsync(c => c.Key == key);

            if (content == null)
                return false;

            // Soft delete
            content.IsActive = false;
            content.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Content>> GetByCategoryAsync(string category)
        {
            return await _context.Contents
                .Where(c => c.Category == category && c.IsActive)
                .OrderBy(c => c.Key)
                .ToListAsync();
        }
    }
}