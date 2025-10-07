using HealthyolBackend.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HealthyolBackend.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Optional relation to Hospital (for hospital users)
        public Guid? HospitalId { get; set; }
        public Hospital? Hospital { get; set; }
    }
}
