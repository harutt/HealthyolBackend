using HealthyolBackend.DTOs;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace HealthyolBackend.Services.Places
{
    

public interface IGooglePlacesClient
    {
        Task<List<string>> TextSearchPlaceIdsAsync(string query, CancellationToken ct);
        Task<PlaceDetails?> GetDetailsAsync(string placeId, CancellationToken ct);
        string BuildPhotoUrl(string photoRef, int maxWidth = 1200);
        string BuildMapsUrl(string placeId);
    }

    public class GooglePlacesClient : IGooglePlacesClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        private const string TEXTSEARCH = "https://maps.googleapis.com/maps/api/place/textsearch/json";
        private const string DETAILS = "https://maps.googleapis.com/maps/api/place/details/json";

        public GooglePlacesClient(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _apiKey = cfg["Google:PlacesApiKey"] ?? throw new InvalidOperationException("Google:PlacesApiKey missing");
        }

        public async Task<List<string>> TextSearchPlaceIdsAsync(string query, CancellationToken ct)
        {
            var placeIds = new List<string>();
            string? next = null;

            do
            {
                var url = $"{TEXTSEARCH}?query={Uri.EscapeDataString(query)}&key={_apiKey}" + (next != null ? $"&pagetoken={next}" : "");
                var rsp = await _http.GetFromJsonAsync<TextSearchResponse>(url, JsonOpts, ct);
                if (rsp?.Results != null)
                    placeIds.AddRange(rsp.Results.Select(r => r.PlaceId).Where(id => !string.IsNullOrWhiteSpace(id)));
                next = rsp?.NextPageToken;
                if (!string.IsNullOrWhiteSpace(next)) await Task.Delay(2200, ct);
            } while (!string.IsNullOrWhiteSpace(next));

            return placeIds.Distinct().ToList();
        }

        public async Task<PlaceDetails?> GetDetailsAsync(string placeId, CancellationToken ct)
        {
            var fields = string.Join(",",
                "place_id", "name", "formatted_address", "geometry/location",
                "international_phone_number", "formatted_phone_number",
                "website", "rating", "user_ratings_total", "address_components", "photos"
            );
            var url = $"{DETAILS}?place_id={Uri.EscapeDataString(placeId)}&fields={fields}&key={_apiKey}";
            var rsp = await _http.GetFromJsonAsync<DetailsResponse>(url, JsonOpts, ct);
            return rsp?.Result;
        }

        public string BuildPhotoUrl(string photoRef, int maxWidth = 1200) =>
            $"https://maps.googleapis.com/maps/api/place/photo?maxwidth={maxWidth}&photo_reference={Uri.EscapeDataString(photoRef)}&key={_apiKey}";

        public string BuildMapsUrl(string placeId) =>
            $"https://www.google.com/maps/search/?api=1&query=Google&query_place_id={Uri.EscapeDataString(placeId)}";
    }
}
