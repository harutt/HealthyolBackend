// Models/Doctor.cs
using System.ComponentModel.DataAnnotations;

namespace HealthyolBackend.Models
{
    public class Doctor
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Specialty { get; set; }
        public int? ExperienceYears { get; set; }

        public List<string> LanguagesSpoken { get; set; } = new();

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }

        // Images
        public string? ProfileImageUrl { get; set; }

        // Rating
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        // Status
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public Guid? HospitalId { get; set; }
        public Hospital? Hospital { get; set; }

        public List<DoctorHealthService> DoctorHealthService { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
    }
}
