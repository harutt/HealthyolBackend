namespace HealthyolBackend.DTOs
{
    using System.Text.Json.Serialization;

    public class TextSearchResponse
    {
        [JsonPropertyName("results")] public List<TextSearchResult> Results { get; set; } = new();
        [JsonPropertyName("next_page_token")] public string? NextPageToken { get; set; }
        [JsonPropertyName("status")] public string Status { get; set; } = "";
    }

    public class TextSearchResult
    {
        [JsonPropertyName("place_id")] public string PlaceId { get; set; } = "";
    }

    public class DetailsResponse
    {
        [JsonPropertyName("result")] public PlaceDetails? Result { get; set; }
        [JsonPropertyName("status")] public string Status { get; set; } = "";
    }

    public class PlaceDetails
    {
        [JsonPropertyName("place_id")] public string PlaceId { get; set; } = "";
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("formatted_address")] public string? FormattedAddress { get; set; }
        [JsonPropertyName("geometry")] public Geometry? Geometry { get; set; }
        [JsonPropertyName("international_phone_number")] public string? InternationalPhoneNumber { get; set; }
        [JsonPropertyName("formatted_phone_number")] public string? FormattedPhoneNumber { get; set; }
        [JsonPropertyName("website")] public string? Website { get; set; }
        [JsonPropertyName("rating")] public double? Rating { get; set; }
        [JsonPropertyName("user_ratings_total")] public int? UserRatingsTotal { get; set; }
        [JsonPropertyName("address_components")] public List<AddressComponent> AddressComponents { get; set; } = new();
        [JsonPropertyName("photos")] public List<PhotoRef> Photos { get; set; } = new();
    }

    public class Geometry { [JsonPropertyName("location")] public Location? Location { get; set; } }
    public class Location { [JsonPropertyName("lat")] public double Lat { get; set; } [JsonPropertyName("lng")] public double Lng { get; set; } }

    public class AddressComponent
    {
        [JsonPropertyName("long_name")] public string LongName { get; set; } = "";
        [JsonPropertyName("short_name")] public string ShortName { get; set; } = "";
        [JsonPropertyName("types")] public List<string> Types { get; set; } = new();
    }

    public class PhotoRef
    {
        [JsonPropertyName("photo_reference")] public string PhotoReference { get; set; } = "";
        [JsonPropertyName("width")] public int Width { get; set; }
        [JsonPropertyName("height")] public int Height { get; set; }
    }
}
