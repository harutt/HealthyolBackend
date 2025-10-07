namespace HealthyolBackend.DTOs
{
    public class HospitalDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? GoogleMapsUrl { get; set; }
        public string? GooglePlacesUrl { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<HealthServiceDto> Services { get; set; } = new();
        public List<DoctorDto> Doctors { get; set; } = new();
    }

    public class CreateHospitalDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? GoogleMapsUrl { get; set; }
        public string? GooglePlacesUrl { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public List<Guid> ServiceIds { get; set; } = new();
        public bool IsFeatured { get; set; } = false;
    }

    public class UpdateHospitalDto : CreateHospitalDto
    {
        public bool IsActive { get; set; } = true;
    }

    public class HospitalSearchDto
    {
        public string? Query { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public List<Guid> ServiceIds { get; set; } = new();
        public double? MinRating { get; set; }
        public bool? IsVerified { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "name"; // name, rating, distance
        public string SortOrder { get; set; } = "asc"; // asc, desc
    }
}