using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HealthyolBackend.Models;
using System.Globalization;
using System.Text;

namespace HealthyolBackend.Data
{
    public static class SeedData
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await context.Database.EnsureCreatedAsync();

            // Roles
            string[] roles = { "Admin", "Hospital", "User" };
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // Admin
            if (await userManager.FindByEmailAsync("admin@healthyol.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@healthyol.com",
                    Email = "admin@healthyol.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Ensure top-level category HealthServices exist (add missing by Name)
            var categories = new (string Name, string Description, string IconKey)[]
            {
                ("Dentistry", "Dental and oral care", "tooth"),
                ("Ophthalmology","Eye and vision care", "eye"),
                ("Oncology", "Cancer diagnosis and treatment","radiation"),
                ("Fertility (IVF)", "IVF and infertility services", "dna"),
                ("Plastic & Reconstructive", "Aesthetic and reconstructive surgery",      "wand-sparkles"),
                ("Orthopedics", "Musculoskeletal care", "bone"),
                ("Cardiology", "Heart and cardiovascular care",  "heart"),
                ("Neurology", "Brain and nervous system care",  "brain"),
                ("Bariatric Surgery", "Obesity and metabolic surgery",  "scale"),
                ("ENT", "Ear, nose, throat care", "ear"),
                ("Dermatology", "Skin, hair and nail care", "sparkles"),
                ("Gastroenterology", "Digestive system care", "stethoscope"),
                ("Hepatology", "Liver disease care", "clipboard-list"),
                ("Endocrinology","Hormones, diabetes, thyroid", "pill"),
                ("Rheumatology", "Autoimmune and joint diseases", "hand"),
                ("Nephrology", "Kidney and hypertension care", "kidney"),
                ("Urology", "Urinary tract and prostate care","flask-round"),
                ("Pulmonology", "Lung and sleep disorders", "lungs"),
                ("Radiology", "Imaging and interventional radiology", "scan"),
                ("Laboratory & Pathology", "Laboratory diagnostics and pathology",       "microscope"),
                ("Emergency Medicine", "24/7 emergency care","ambulance"),
                ("Pediatrics", "Child health and surgery", "baby"),
                ("Obstetrics & Gynecology", "Women’s health and pregnancy", "female"),
                ("Neurosurgery", "Brain and spine surgery", "brain-cog"),
                ("Cardiothoracic Surgery", "Heart and chest surgery", "activity"),
                ("Rehabilitation", "Physiotherapy and rehab", "wheelchair"),
                ("General Surgery", "General surgical procedures", "scalpel"),
                ("Transplant", "Organ transplant programs", "recycle"),
                ("Check-Up", "Comprehensive health check-up", "clipboard-check"),
};

            // Load existing names for idempotent add
            var existingServiceNames = await context.HealthServices
                .AsNoTracking()
                .Select(s => s.Name)
                .ToListAsync();

            var existingSet = new HashSet<string>(existingServiceNames, StringComparer.OrdinalIgnoreCase);

            var toAdd = new List<HealthService>();
            foreach (var (name, desc, icon) in categories)
            {
                if (existingSet.Contains(name)) continue;
                toAdd.Add(new HealthService
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = desc,
                    IconKey = icon,
                    Category = name,           // Category = top-level category name
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (toAdd.Count > 0)
            {
                context.HealthServices.AddRange(toAdd);
                await context.SaveChangesAsync(); // IMPORTANT: persist before linking
            }

            // Link hospitals <-> category services via heuristics (works with your existing Google Places hospitals)
            await SeedHospitalServiceLinksAsync(context);

            // Seed content (unchanged)
            if (!await context.Contents.AnyAsync())
            {
                var contents = new List<Content>
                {
                    new() { Id = Guid.NewGuid(), Key="home_title",    Title="Home Page Title",    Value="Find the Best Hospitals Near You", ContentType="text", Category="homepage", CreatedBy="system", UpdatedBy="system", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow, IsActive=true },
                    new() { Id = Guid.NewGuid(), Key="home_subtitle", Title="Home Page Subtitle", Value="Connect with top-rated hospitals and healthcare providers", ContentType="text", Category="homepage", CreatedBy="system", UpdatedBy="system", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow, IsActive=true }
                };
                context.Contents.AddRange(contents);
            }

            await context.SaveChangesAsync();
        }

        // Heuristic linker: matches hospital name/desc to category service by TR/EN keywords
        private static async Task SeedHospitalServiceLinksAsync(ApplicationDbContext context)
        {
            var services = await context.HealthServices.AsNoTracking().ToListAsync();
            var hospitals = await context.Hospitals.AsNoTracking().ToListAsync();
            if (!services.Any() || !hospitals.Any()) return;

            var existing = await context.HospitalServices
                .AsNoTracking()
                .Select(hs => new { hs.HospitalId, hs.HealthServiceId })
                .ToListAsync();
            var linkSet = new HashSet<(Guid hospitalId, Guid serviceId)>(existing.Select(x => (x.HospitalId, x.HealthServiceId)));

            // Map group → target service Name
            var groupToServiceName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["dental"] = "Dentistry",
                ["goz"] = "Ophthalmology",
                ["onkoloji"] = "Oncology",
                ["ivf"] = "Fertility (IVF)",
                ["estetik"] = "Plastic & Reconstructive",
                ["ortopedi"] = "Orthopedics",
                ["kalp"] = "Cardiology",
                ["neuro"] = "Neurology",
                ["bariatrik"] = "Bariatric Surgery",
                ["ent"] = "ENT",
                ["derm"] = "Dermatology",
                ["gastro"] = "Gastroenterology",
                ["hepato"] = "Hepatology",
                ["endo"] = "Endocrinology",
                ["rheuma"] = "Rheumatology",
                ["nephro"] = "Nephrology",
                ["uro"] = "Urology",
                ["pulmo"] = "Pulmonology",
                ["radio"] = "Radiology",
                ["lab"] = "Laboratory & Pathology",
                ["acil"] = "Emergency Medicine",
                ["pediatri"] = "Pediatrics",
                ["kadin"] = "Obstetrics & Gynecology",
                ["noro-cerr"] = "Neurosurgery",
                ["kvc"] = "Cardiothoracic Surgery",
                ["rehab"] = "Rehabilitation",
                ["genel-cerrahi"] = "General Surgery",
                ["nakil"] = "Transplant",
                ["checkup"] = "Check-Up",
            };

            // Group → keywords (TR/EN) — diacritics-insensitive matching
            var groupToKeywords = new Dictionary<string, string[]>
            {
                ["dental"] = new[] { "dental", "dent", "diş", "dis", "ağız", "agiz", "tooth", "teeth", "implant", "ortodonti", "orthodont" },
                ["goz"] = new[] { "göz", "goz", "ophthalm", "eye", "retina", "cataract", "lasik", "smile", "glaucoma" },
                ["onkoloji"] = new[] { "onkoloji", "onco", "cancer", "kanser", "hematoloji", "hematology", "nukleer", "nükleer", "pet/ct", "pet ct", "radyoterapi", "radiation" },
                ["ivf"] = new[] { "ivf", "tüp bebek", "tup bebek", "fertility", "icsi", "embryo", "infertility", "andrology" },
                ["estetik"] = new[] { "estetik", "aesthetic", "cosmetic", "plastik", "plastic", "rinoplasti", "rhinoplasty", "liposuction", "bleph", "dermat", "botox", "filler", "abdominoplasty", "meme", "breast lift" },
                ["ortopedi"] = new[] { "ortopedi", "ortho", "bone", "joint", "spine", "bel", "boyun", "acl", "meniskus", "sports", "omuz", "shoulder", "kalça", "hip", "knee" },
                ["kalp"] = new[] { "kalp", "heart", "cardio", "bypass", "tavi", "stent", "ekg", "holter", "ekokardiyografi", "echo", "angiography", "cath" },
                ["neuro"] = new[] { "nöro", "neuro", "beyin", "sinir", "epilepsi", "stroke", "spine", "tumor", "tümör", "gamma knife", "parkinson" },
                ["bariatrik"] = new[] { "bariatrik", "bariatric", "obezite", "obesity", "metabolic", "weight", "sleeve", "bypass", "rygb" },
                ["ent"] = new[] { "kbb", "ent", "ear", "nose", "throat", "rinoloji", "sinus", "tonsil", "adenoid" },
                ["derm"] = new[] { "dermatoloji", "dermatology", "cilt", "skin", "botox", "filler", "laser", "akne", "psoria" },
                ["gastro"] = new[] { "gastro", "gastroscopy", "egd", "reflux", "colonoscopy", "ibd", "ibs", "endoscopy", "gi" },
                ["hepato"] = new[] { "hepatoloji", "hepatology", "hepatit", "siroz", "cirrhosis", "liver", "hcc" },
                ["endo"] = new[] { "endokrin", "endocrin", "diabetes", "thyroid", "adrenal", "pituitary" },
                ["rheuma"] = new[] { "romatoloji", "rheumat", "ra", "ankylosing", "spondyl", "lupus", "vascul" },
                ["nephro"] = new[] { "nefro", "nephro", "kidney", "dialysis", "hemodialysis", "ckd", "renal" },
                ["uro"] = new[] { "üroloji", "uro", "prostate", "bph", "stone", "kidney stone", "eswl", "turp" },
                ["pulmo"] = new[] { "göğüs", "gogus", "pulmo", "asthma", "copd", "sleep", "polysomnography", "broncho" },
                ["radio"] = new[] { "radyoloji", "radiology", "mri", "ct", "ultrasound", "doppler", "interventional" },
                ["lab"] = new[] { "laboratuvar", "laboratory", "pathology", "histopath", "ihc", "genetic", "biopsy" },
                ["acil"] = new[] { "acil", "emergency", "trauma", "24/7", "24 7", "ambulance" },
                ["pediatri"] = new[] { "çocuk", "cocuk", "pediatr", "newborn", "neonatal" },
                ["kadin"] = new[] { "kadın doğum", "kadin dogum", "jinekoloji", "obgyn", "gynec", "pregnancy", "mfm", "ivf" },
                ["noro-cerr"] = new[] { "beyin cerrahisi", "neurosurgery", "spine surgery", "disk", "aneurysm" },
                ["kvc"] = new[] { "kalp damar cerrahisi", "cardiothoracic", "cabg", "valve surgery", "tavi" },
                ["rehab"] = new[] { "ftr", "fizik tedavi", "rehab", "physiotherapy", "stroke rehab", "sports rehab" },
                ["genel-cerrahi"] = new[] { "genel cerrahi", "general surgery", "herni", "cholecyst", "thyroid", "appendix" },
                ["nakil"] = new[] { "organ nakli", "transplant", "kidney transplant", "liver transplant" },
                ["checkup"] = new[] { "check-up", "check up", "checkup", "tarama", "screening" },
            };

            // Quick lookup by service Name (case-insensitive)
            var serviceByName = services
                .GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            int newLinks = 0;

            foreach (var hospital in hospitals)
            {
                var text = Normalize($"{hospital.Name} {hospital.Description}");

                var matchedGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var kv in groupToKeywords)
                    if (kv.Value.Any(k => text.Contains(Normalize(k))))
                        matchedGroups.Add(kv.Key);

                if (!matchedGroups.Any())
                    continue;

                foreach (var group in matchedGroups)
                {
                    if (!groupToServiceName.TryGetValue(group, out var serviceName)) continue;
                    if (!serviceByName.TryGetValue(serviceName, out var svc)) continue;

                    var key = (hospital.Id, svc.Id);
                    if (linkSet.Contains(key)) continue;

                    context.HospitalServices.Add(new HospitalHealthService
                    {
                        Id = Guid.NewGuid(),
                        HospitalId = hospital.Id,
                        HealthServiceId = svc.Id,
                        Description = $"Available at {hospital.Name}",
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    });

                    linkSet.Add(key);
                    newLinks++;
                }
            }

            if (newLinks > 0)
                await context.SaveChangesAsync();
        }

        // Lowercase & remove diacritics for robust matching
        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var lower = value.ToLowerInvariant();
            var normalized = lower.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}