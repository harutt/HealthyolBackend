using HealthyolBackend.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HealthyolBackend.Models
{
    public class Hospital
    {
        public Guid Id { get; set; }

        // Core info
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        public string? GoogleMapsUrl { get; set; }
        public string? GooglePlacesUrl { get; set; }

        // Optional info
        [MaxLength(20)]
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }

        // Images
        public string? LogoUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();

        // Rating and reviews
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        // Status
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public List<HealthService> HealthService { get; set; } = new();
        public List<Doctor> Doctors { get; set; } = new();
        public List<HospitalHealthService> HospitalHealthService { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
    }
}
