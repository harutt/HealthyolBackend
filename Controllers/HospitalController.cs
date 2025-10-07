using HealthyolBackend.DTOs;
using HealthyolBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthyolBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HospitalsController : ControllerBase
    {
        private readonly IHospitalService _hospitalService;

        public HospitalsController(IHospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        /// <summary>
        /// Get all hospitals
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HospitalDto>>> GetHospitals()
        {
            var hospitals = await _hospitalService.GetAllAsync();
            return Ok(hospitals);
        }

        /// <summary>
        /// Get hospital by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<HospitalDto>> GetHospital(Guid id)
        {
            var hospital = await _hospitalService.GetByIdAsync(id);
            if (hospital == null)
                return NotFound();

            return Ok(hospital);
        }

        /// <summary>
        /// Search hospitals with filters
        /// </summary>
        [HttpPost("search")]
        public async Task<ActionResult<object>> SearchHospitals(HospitalSearchDto searchDto)
        {
            var (hospitals, totalCount) = await _hospitalService.SearchAsync(searchDto);

            return Ok(new
            {
                hospitals,
                totalCount,
                page = searchDto.Page,
                pageSize = searchDto.PageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
            });
        }

        /// <summary>
        /// Create a new hospital (Admin or Hospital role required)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Hospital")]
        public async Task<ActionResult<HospitalDto>> CreateHospital(CreateHospitalDto createDto)
        {
            var hospital = await _hospitalService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetHospital), new { id = hospital.Id }, hospital);
        }

        /// <summary>
        /// Update hospital (Admin or Hospital role required)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Hospital")]
        public async Task<ActionResult<HospitalDto>> UpdateHospital(Guid id, UpdateHospitalDto updateDto)
        {
            var hospital = await _hospitalService.UpdateAsync(id, updateDto);
            if (hospital == null)
                return NotFound();

            return Ok(hospital);
        }

        /// <summary>
        /// Delete hospital (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteHospital(Guid id)
        {
            var result = await _hospitalService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Add service to hospital
        /// </summary>
        [HttpPost("{id}/services/{serviceId}")]
        [Authorize(Roles = "Admin,Hospital")]
        public async Task<ActionResult> AddServiceToHospital(Guid id, Guid serviceId, [FromBody] AddServiceDto? dto = null)
        {
            var result = await _hospitalService.AddServiceAsync(id, serviceId, dto?.Description, dto?.Price);
            if (!result)
                return BadRequest("Service already exists for this hospital or invalid IDs");

            return NoContent();
        }

        /// <summary>
        /// Remove service from hospital
        /// </summary>
        [HttpDelete("{id}/services/{serviceId}")]
        [Authorize(Roles = "Admin,Hospital")]
        public async Task<ActionResult> RemoveServiceFromHospital(Guid id, Guid serviceId)
        {
            var result = await _hospitalService.RemoveServiceAsync(id, serviceId);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Get all linked services for a hospital
        /// </summary>
        [HttpGet("{id}/services")]
        [AllowAnonymous] // adjust if you want to restrict
        public async Task<ActionResult<IEnumerable<HealthServiceDto>>> GetHospitalServices(Guid id)
        {
            var services = await _hospitalService.GetServicesAsync(id);
            return Ok(services);
        }
    }

    public class AddServiceDto
    {
        public string? Description { get; set; }
        public decimal? Price { get; set; }
    }
}