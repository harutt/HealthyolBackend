namespace HealthyolBackend.Configuration
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; } = new();
        public JwtSettings JwtSettings { get; set; } = new();
        public GoogleSettings Google { get; set; } = new();
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; } = string.Empty;
    }

    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryInHours { get; set; } = 24;
    }

    public class GoogleSettings
    {
        public string PlacesApiKey { get; set; } = string.Empty;
    }
}