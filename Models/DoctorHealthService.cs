using System;

namespace HealthyolBackend.Models
{
    public class DoctorHealthService
    {
        public Guid Id { get; set; }

        public Guid DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public Guid HealthServiceId { get; set; }
        public HealthService HealthService { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}