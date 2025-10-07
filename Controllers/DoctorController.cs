using HealthyolBackend.DTOs;
using HealthyolBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthyolBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        /// <summary>
        /// Get all active doctors
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetAllDoctors()
        {
            var doctors = await _doctorService.GetAllAsync();
            return Ok(doctors);
        }

        /// <summary>
        /// Get doctor by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorDto>> GetDoctor(Guid id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }

        /// <summary>
        /// Create new doctor (Admin or Hospital role required)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Hospital")]
        public async Task<ActionResult<DoctorDto>> CreateDoctor([FromBody] CreateDoctorDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var doctor = await _doctorService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.Id }, doctor);
        }

        /// <summary>
        /// Update doctor (Admin or Hospital role required)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Hospital")]
        public async Task<ActionResult<DoctorDto>> UpdateDoctor(Guid id, [FromBody] UpdateDoctorDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var doctor = await _doctorService.UpdateAsync(id, updateDto);
            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }

        /// <summary>
        /// Delete doctor (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteDoctor(Guid id)
        {
            var result = await _doctorService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Get doctors by hospital ID
        /// </summary>
        [HttpGet("hospital/{hospitalId}")]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetDoctorsByHospital(Guid hospitalId)
        {
            var doctors = await _doctorService.GetByHospitalIdAsync(hospitalId);
            return Ok(doctors);
        }

        /// <summary>
        /// Search doctors by name, specialty, or description
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> SearchDoctors([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query cannot be empty");

            var doctors = await _doctorService.SearchAsync(query);
            return Ok(doctors);
        }
    }
}