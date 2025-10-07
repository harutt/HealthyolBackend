using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HealthyolBackend.Models;
using System.Text.Json;

namespace HealthyolBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<HealthService> HealthServices { get; set; }
        public DbSet<HospitalHealthService> HospitalServices { get; set; }
        public DbSet<DoctorHealthService> DoctorServices { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Content> Contents { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Hospital configuration
            builder.Entity<Hospital>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.ImageUrls)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>());

                entity.HasMany(h => h.Doctors)
                      .WithOne(d => d.Hospital)
                      .HasForeignKey(d => d.HospitalId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(h => h.Reviews)
                      .WithOne(r => r.Hospital)
                      .HasForeignKey(r => r.HospitalId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Doctor configuration
            builder.Entity<Doctor>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.LanguagesSpoken)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>());

                entity.HasMany(d => d.Reviews)
                      .WithOne(r => r.Doctor)
                      .HasForeignKey(r => r.DoctorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Service configuration
            builder.Entity<HealthService>(entity =>
            {
                entity.HasKey(s => s.Id);
            });

            // HospitalService configuration (Many-to-Many with additional properties)
            builder.Entity<HospitalHealthService>(entity =>
            {
                entity.HasKey(hs => hs.Id);

                entity.HasOne(hs => hs.Hospital)
                      .WithMany(h => h.HospitalHealthService)
                      .HasForeignKey(hs => hs.HospitalId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(hs => hs.HealthService)
                      .WithMany(s => s.HospitalHealthService)
                      .HasForeignKey(hs => hs.HealthServiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // DoctorServices configuration
            builder.Entity<DoctorHealthService>(entity =>
            {
                entity.HasKey(ds => ds.Id);

                entity.HasOne(ds => ds.Doctor)
                      .WithMany(d => d.DoctorHealthService)
                      .HasForeignKey(ds => ds.DoctorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ds => ds.HealthService)
                      .WithMany(s => s.DoctorHealthService)
                      .HasForeignKey(ds => ds.HealthServiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Review configuration
            builder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Content configuration
            builder.Entity<Content>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.Key).IsUnique();
            });

            // ApplicationUser configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(u => u.Hospital)
                      .WithMany()
                      .HasForeignKey(u => u.HospitalId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}