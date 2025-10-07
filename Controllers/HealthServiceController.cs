using HealthyolBackend.DTOs;
using HealthyolBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthyolBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthServiceController : ControllerBase
    {
        private readonly IHealthServiceService _healthServiceService;

        public HealthServiceController(IHealthServiceService healthServiceService)
        {
            _healthServiceService = healthServiceService;
        }

        /// <summary>
        /// Get all active health services
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HealthServiceDto>>> GetAllServices()
        {
            var services = await _healthServiceService.GetAllAsync();
            return Ok(services);
        }

        /// <summary>
        /// Get health service by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<HealthServiceDto>> GetService(Guid id)
        {
            var service = await _healthServiceService.GetByIdAsync(id);
            if (service == null)
                return NotFound();

            return Ok(service);
        }

        /// <summary>
        /// Create new health service (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HealthServiceDto>> CreateService([FromBody] CreateHealthServiceDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = await _healthServiceService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetService), new { id = service.Id }, service);
        }

        /// <summary>
        /// Update health service (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HealthServiceDto>> UpdateService(Guid id, [FromBody] UpdateHealthServiceDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = await _healthServiceService.UpdateAsync(id, updateDto);
            if (service == null)
                return NotFound();

            return Ok(service);
        }

        /// <summary>
        /// Delete health service (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteService(Guid id)
        {
            var result = await _healthServiceService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Get health services by category
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<HealthServiceDto>>> GetServicesByCategory(string category)
        {
            var services = await _healthServiceService.GetByCategoryAsync(category);
            return Ok(services);
        }
    }
}