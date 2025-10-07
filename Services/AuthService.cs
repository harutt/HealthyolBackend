using HealthyolBackend.DTOs;
using HealthyolBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HealthyolBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return null;

            var token = await GenerateJwtTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponseDto
            {
                Token = token,
                Expires = DateTime.UtcNow.AddHours(double.Parse(_configuration.GetSection("JwtSettings")["ExpiryInHours"]!)),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList(),
                    HospitalId = user.HospitalId
                }
            };
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return null;

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                HospitalId = registerDto.HospitalId,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
                return null;

            await _userManager.AddToRoleAsync(user, registerDto.Role);

            var token = await GenerateJwtTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponseDto
            {
                Token = token,
                Expires = DateTime.UtcNow.AddHours(double.Parse(_configuration.GetSection("JwtSettings")["ExpiryInHours"]!)),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList(),
                    HospitalId = user.HospitalId
                }
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList(),
                HospitalId = user.HospitalId
            };
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim("sub", user.Id), // Subject claim for JWT standard
                new Claim("email", user.Email!), // Alternative email claim
                new Claim("FirstName", user.FirstName ?? ""),
                new Claim("LastName", user.LastName ?? ""),
                new Claim("userId", user.Id), // Alternative user ID claim
                new Claim("username", user.Email!) // Username claim
            };

            // Add hospital ID if user has one
            if (user.HospitalId.HasValue)
            {
                claims.Add(new Claim("hospitalId", user.HospitalId.Value.ToString()));
            }

            // Add role claims in multiple formats for maximum compatibility
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role)); // Standard role claim
                claims.Add(new Claim("role", role)); // Alternative role claim
                claims.Add(new Claim("roles", role)); // Alternative roles claim
            }

            // Add a combined roles claim as JSON array for frontend consumption
            if (roles.Any())
            {
                claims.Add(new Claim("rolesList", string.Join(",", roles)));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(double.Parse(jwtSettings["ExpiryInHours"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}