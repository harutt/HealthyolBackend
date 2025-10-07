using HealthyolBackend.Services.Places;
using Microsoft.AspNetCore.Mvc;

namespace HealthyolBackend.Controllers
{
    [ApiController]
    [Route("api/admin/ingest")]
    public class AdminIngestController : ControllerBase
    {
        private readonly IHospitalIngestionService _ing;
        public AdminIngestController(IHospitalIngestionService ing) => _ing = ing;

        [HttpPost("hospitals")]
        public async Task<IActionResult> Ingest([FromQuery] string cities = "İstanbul,Ankara,İzmir,Antalya,Bursa", [FromQuery] string country = "Türkiye")
        {
            var list = cities.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var added = await _ing.IngestCitiesAsync(list, country, HttpContext.RequestAborted);
            return Ok(new { added, cities = list });
        }
    }
}
