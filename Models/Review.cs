using System;
using System.ComponentModel.DataAnnotations;

namespace HealthyolBackend.Models
{
    public class Review
    {
        public Guid Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        // Either Hospital or Doctor review
        public Guid? HospitalId { get; set; }
        public Hospital? Hospital { get; set; }

        public Guid? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = false;
    }
}