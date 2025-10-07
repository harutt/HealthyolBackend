using HealthyolBackend.DTOs;

namespace HealthyolBackend.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<UserDto?> GetUserByIdAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> AssignRoleAsync(string userId, string role);
    }
}