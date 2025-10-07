namespace HealthyolBackend.DTOs
{
    public class DoctorDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public int? ExperienceYears { get; set; }
        public List<string> LanguagesSpoken { get; set; } = new();
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public string? ProfileImageUrl { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; } // NEW
        public DateTime CreatedAt { get; set; }

        public Guid? HospitalId { get; set; }
        public string? HospitalName { get; set; }
        public List<HealthServiceDto> Services { get; set; } = new();
    }

    public class CreateDoctorDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public int? ExperienceYears { get; set; }
        public List<string> LanguagesSpoken { get; set; } = new();
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public string? ProfileImageUrl { get; set; }
        public Guid? HospitalId { get; set; }
        public List<Guid> ServiceIds { get; set; } = new();
        public bool IsFeatured { get; set; } = false; // NEW
    }

    public class UpdateDoctorDto : CreateDoctorDto
    {
        public bool IsActive { get; set; } = true;
    }
}