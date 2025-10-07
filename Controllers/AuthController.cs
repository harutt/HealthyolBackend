using HealthyolBackend.DTOs;
using HealthyolBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthyolBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
                return Unauthorized("Invalid credentials");

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(registerDto);
            if (result == null)
                return BadRequest("Registration failed. User may already exist.");

            return Ok(result);
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Debug endpoint to verify JWT claims are correctly parsed
        /// </summary>
        [HttpGet("debug-claims")]
        [Authorize]
        public IActionResult GetDebugClaims()
        {
            var allClaims = User.Claims.Select(c => new { 
                Type = c.Type, 
                Value = c.Value 
            }).ToList();
            
            var roleClaims = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isAdminCapital = User.IsInRole("Admin");
            var isAdminLower = User.IsInRole("admin");
            
            return Ok(new
            {
                Message = "✅ JWT Claims Debug Information",
                AllClaims = allClaims,
                RoleClaims = roleClaims,
                RoleTests = new
                {
                    IsAdmin_Capital = isAdminCapital,
                    IsAdmin_Lowercase = isAdminLower,
                    HasAnyRoles = roleClaims.Any()
                },
                UserInfo = new
                {
                    UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UserName = User.FindFirst(ClaimTypes.Name)?.Value,
                    Email = User.FindFirst(ClaimTypes.Email)?.Value
                },
                ClaimTypes_Used = new
                {
                    Role = ClaimTypes.Role,
                    NameIdentifier = ClaimTypes.NameIdentifier,
                    Name = ClaimTypes.Name,
                    Email = ClaimTypes.Email
                }
            });
        }

        /// <summary>
        /// Test admin-only endpoint to verify role-based authorization
        /// </summary>
        [HttpGet("admin-test")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminTest()
        {
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            
            return Ok(new { 
                Message = "🎉 SUCCESS! Admin access confirmed!",
                UserName = User.Identity?.Name,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                UserRoles = userRoles,
                Timestamp = DateTime.UtcNow,
                Note = "If you see this message, role claims are working correctly!"
            });
        }

        /// <summary>
        /// Test any authenticated user endpoint
        /// </summary>
        [HttpGet("user-test")]
        [Authorize]
        public IActionResult UserTest()
        {
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            
            return Ok(new { 
                Message = "✅ Authenticated user access confirmed!",
                UserName = User.Identity?.Name,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                UserRoles = userRoles,
                IsAuthenticated = User.Identity?.IsAuthenticated,
                AuthenticationType = User.Identity?.AuthenticationType
            });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!result)
                return BadRequest("Failed to change password");

            return Ok("Password changed successfully");
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto assignRoleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.AssignRoleAsync(assignRoleDto.UserId, assignRoleDto.Role);
            if (!result)
                return BadRequest("Failed to assign role");

            return Ok("Role assigned successfully");
        }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class AssignRoleDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}