using System;
using System.ComponentModel.DataAnnotations;

namespace HealthyolBackend.Models
{
    public class Content
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ContentType { get; set; } = "text"; // text, html, json

        [MaxLength(100)]
        public string? Category { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}