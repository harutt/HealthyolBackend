using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HealthyolBackend.Models
{
    public class HealthService
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Icon to display in frontend
        [MaxLength(50)]
        public string? IconKey { get; set; }
        public string? IconUrl { get; set; }

        // Image to display in frontend
        public string? ImageUrl { get; set; }

        // Category
        [MaxLength(100)]
        public string? Category { get; set; }

        // Status
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public List<HospitalHealthService> HospitalHealthService { get; set; } = new();
        public List<DoctorHealthService> DoctorHealthService { get; set; } = new();
    }
}
