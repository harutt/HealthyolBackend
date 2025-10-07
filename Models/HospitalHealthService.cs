using System;
using System.ComponentModel.DataAnnotations;

namespace HealthyolBackend.Models
{
    public class HospitalHealthService
    {
        public Guid Id { get; set; }

        public Guid HospitalId { get; set; }
        public Hospital Hospital { get; set; } = null!;

        public Guid HealthServiceId { get; set; }
        public HealthService HealthService { get; set; } = null!;

        // Service-specific details for this hospital
        public string? Description { get; set; }
        public decimal? Price { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; } = "TL";

        public bool IsAvailable { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}