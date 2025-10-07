using HealthyolBackend.Data;
using HealthyolBackend.DTOs;
using HealthyolBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthyolBackend.Services.Places
{
    public interface IHospitalIngestionService
    {
        Task<int> IngestCityAsync(string city, string country, CancellationToken ct);
        Task<int> IngestCitiesAsync(IEnumerable<string> cities, string country, CancellationToken ct);
    }

    public class HospitalIngestionService : IHospitalIngestionService
    {
        private readonly ApplicationDbContext _db;
        private readonly IGooglePlacesClient _places;

        public HospitalIngestionService(ApplicationDbContext db, IGooglePlacesClient places)
        {
            _db = db;
            _places = places;
        }

        private static (string? City, string? Country) ParseCityCountry(IEnumerable<AddressComponent> comps)
        {
            string? By(string t) => comps.FirstOrDefault(c => c.Types.Contains(t))?.LongName;
            var city = By("locality") ?? By("administrative_area_level_2") ?? By("administrative_area_level_1");
            var country = By("country");
            return (city, country);
        }

        public async Task<int> IngestCityAsync(string city, string country, CancellationToken ct)
        {
            var query = $"hospitals in {city}, {country}";
            var placeIds = await _places.TextSearchPlaceIdsAsync(query, ct);
            if (placeIds.Count == 0) return 0;

            // Load existing googleMapsUrls for dedupe
            var existingUrls = await _db.Hospitals
                .Where(h => h.GoogleMapsUrl != null)
                .Select(h => h.GoogleMapsUrl!)
                .ToListAsync(ct);

            var existingNameCity = await _db.Hospitals
                .Select(h => new { h.Name, h.City })
                .ToListAsync(ct);

            var toInsert = new List<Hospital>();

            foreach (var pid in placeIds)
            {
                var det = await _places.GetDetailsAsync(pid, ct);
                if (det?.Name == null) continue;

                var mapsUrl = _places.BuildMapsUrl(det.PlaceId);
                if (existingUrls.Contains(mapsUrl)) continue; // dedupe by Maps URL

                var (parsedCity, parsedCountry) = ParseCityCountry(det.AddressComponents ?? new());
                var effectiveCity = parsedCity ?? city;
                var effectiveCountry = parsedCountry ?? country;

                // secondary dedupe by Name + City (case-insensitive)
                if (existingNameCity.Any(x =>
                        string.Equals(x.Name, det.Name, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.City, effectiveCity, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var imageUrls = new List<string>();
                if (det.Photos != null && det.Photos.Count > 0)
                {
                    foreach (var p in det.Photos.Take(3))
                        imageUrls.Add(_places.BuildPhotoUrl(p.PhotoReference, Math.Min(1600, Math.Max(600, p.Width))));
                }

                var phone = det.InternationalPhoneNumber ?? det.FormattedPhoneNumber;

                var h = new Hospital
                {
                    Id = Guid.NewGuid(),
                    Name = det.Name,
                    Email = null,
                    Address = det.FormattedAddress,
                    City = effectiveCity,
                    Country = effectiveCountry,
                    GoogleMapsUrl = mapsUrl,
                    GooglePlacesUrl = mapsUrl, // şeman bozulmasın diye aynı URL
                    Phone = phone,
                    Website = det.Website,
                    Description = null,
                    LogoUrl = null,
                    ImageUrls = imageUrls,
                    AverageRating = det.Rating ?? 0,
                    ReviewCount = det.UserRatingsTotal ?? 0,
                    IsActive = true,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                toInsert.Add(h);
            }

            if (toInsert.Count > 0)
            {
                _db.Hospitals.AddRange(toInsert);
                await _db.SaveChangesAsync(ct);
            }

            return toInsert.Count;
        }

        public async Task<int> IngestCitiesAsync(IEnumerable<string> cities, string country, CancellationToken ct)
        {
            var total = 0;
            foreach (var c in cities)
                total += await IngestCityAsync(c, country, ct);
            return total;
        }
    }
}
