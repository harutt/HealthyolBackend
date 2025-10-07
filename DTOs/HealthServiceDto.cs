namespace HealthyolBackend.DTOs
{
    public class HealthServiceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconKey { get; set; }
        public string? IconUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; } // NEW
        public DateTime CreatedAt { get; set; }
    }

    public class CreateHealthServiceDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconKey { get; set; }
        public string? IconUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public bool IsFeatured { get; set; } = false; // allow admin to set on create
    }

    public class UpdateHealthServiceDto : CreateHealthServiceDto
    {
        public bool IsActive { get; set; } = true;
    }
}