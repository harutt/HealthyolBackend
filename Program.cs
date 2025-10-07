using HealthyolBackend.Data;
using HealthyolBackend.Models;
using HealthyolBackend.Services;
using HealthyolBackend.Services.Places;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Entity Framework - Support environment variables
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
                       ?? throw new InvalidOperationException("Connection string not found");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication - Support environment variables
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtSecret = jwtSettings["Secret"] 
               ?? Environment.GetEnvironmentVariable("JWT_SECRET")
               ?? throw new InvalidOperationException("JWT Secret not found");

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER"),
        ValidAudience = jwtSettings["Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };

    // Normalize role claims
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                var currentRoleValues = identity.FindAll(ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                var incomingRoles = identity.FindAll("roles").Select(c => c.Value)
                    .Concat(identity.FindAll("role").Select(c => c.Value))
                    .Concat(currentRoleValues)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var finalRolesCI = new HashSet<string>(currentRoleValues, StringComparer.OrdinalIgnoreCase);
                var currentRoleClaims = identity.FindAll(ClaimTypes.Role).ToList();

                var canonical = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["admin"] = "Admin",
                    ["hospital"] = "Hospital",
                    ["user"] = "User"
                };

                foreach (var role in incomingRoles)
                {
                    if (!finalRolesCI.Contains(role))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                        finalRolesCI.Add(role);
                        currentRoleClaims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    if (canonical.TryGetValue(role, out var canon))
                    {
                        bool hasCanonicalExact = currentRoleClaims.Any(c => c.Value == canon);
                        if (!hasCanonicalExact)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, canon));
                            finalRolesCI.Add(canon);
                            currentRoleClaims.Add(new Claim(ClaimTypes.Role, canon));
                        }
                    }
                }

                if (!identity.HasClaim(c => c.Type == ClaimTypes.Name))
                {
                    var name = identity.FindFirst("name")?.Value
                               ?? identity.FindFirst("username")?.Value
                               ?? identity.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrWhiteSpace(name))
                        identity.AddClaim(new Claim(ClaimTypes.Name, name));
                }
            }
            return Task.CompletedTask;
        }
    };
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("HospitalOnly", policy => policy.RequireRole("Hospital"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

// CORS - Environment-based origins
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                    ?? new[] {
                        "http://localhost:3000",
                        "https://localhost:3000",
                        "http://localhost:5173",
                        "https://localhost:5173",
                        "http://localhost:5174",
                        "https://localhost:5174",
                        "http://localhost:5175",
                        "https://localhost:5175"
                    };

// Add production domains from environment
var productionOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',');
if (productionOrigins != null)
{
    allowedOrigins = allowedOrigins.Concat(productionOrigins).ToArray();
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins(allowedOrigins)
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

// Custom services
builder.Services.AddScoped<IHospitalService, HospitalService>();
builder.Services.AddScoped<IDoctorService, HealthyolBackend.Services.DoctorService>();
builder.Services.AddScoped<IHealthServiceService, HealthServiceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IContentService, ContentService>();

// Configure Google Places Client with environment variable support
builder.Services.AddHttpClient<IGooglePlacesClient, GooglePlacesClient>(client =>
{
    var apiKey = builder.Configuration.GetSection("Google")["PlacesApiKey"]
                ?? Environment.GetEnvironmentVariable("GOOGLE_PLACES_API_KEY");
    
    if (string.IsNullOrEmpty(apiKey) && builder.Environment.IsProduction())
    {
        throw new InvalidOperationException("Google Places API Key not found");
    }
});

builder.Services.AddScoped<IHospitalIngestionService, HospitalIngestionService>();

// Swagger - Only in Development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Healthyol API",
            Version = "v1",
            Description = "Hospital listing and management API"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Healthyol API v1");
        options.RoutePrefix = "swagger";
    });
    
    // Redirect root → Swagger in development only
    app.MapGet("/", () => Results.Redirect("/swagger"));
}
else
{
    // Production error handling
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Only migrate in development or when explicitly configured
    if (app.Environment.IsDevelopment() || 
        Environment.GetEnvironmentVariable("AUTO_MIGRATE") == "true")
    {
        dbContext.Database.Migrate();
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await SeedData.Initialize(dbContext, userManager, roleManager);
}

app.Run();
