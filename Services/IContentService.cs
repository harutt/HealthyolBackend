using HealthyolBackend.Models;

namespace HealthyolBackend.Services
{
    public interface IContentService
    {
        Task<IEnumerable<Content>> GetAllAsync();
        Task<Content?> GetByKeyAsync(string key);
        Task<Content> CreateAsync(Content content, string userId);
        Task<Content?> UpdateAsync(string key, string value, string userId);
        Task<bool> DeleteAsync(string key);
        Task<IEnumerable<Content>> GetByCategoryAsync(string category);
    }
}