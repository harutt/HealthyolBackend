using HealthyolBackend.Models;
using HealthyolBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthyolBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContentController : ControllerBase
    {
        private readonly IContentService _contentService;

        public ContentController(IContentService contentService)
        {
            _contentService = contentService;
        }

        /// <summary>
        /// Get all active content
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Content>>> GetAllContent()
        {
            var content = await _contentService.GetAllAsync();
            return Ok(content);
        }

        /// <summary>
        /// Get content by key
        /// </summary>
        [HttpGet("{key}")]
        public async Task<ActionResult<Content>> GetContentByKey(string key)
        {
            var content = await _contentService.GetByKeyAsync(key);
            if (content == null)
                return NotFound();

            return Ok(content);
        }

        /// <summary>
        /// Create new content (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Content>> CreateContent([FromBody] Content content)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var createdContent = await _contentService.CreateAsync(content, userId);
            
            return CreatedAtAction(nameof(GetContentByKey), new { key = createdContent.Key }, createdContent);
        }

        /// <summary>
        /// Update content value by key (Admin only)
        /// </summary>
        [HttpPut("{key}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Content>> UpdateContent(string key, [FromBody] UpdateContentDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var content = await _contentService.UpdateAsync(key, updateDto.Value, userId);
            
            if (content == null)
                return NotFound();

            return Ok(content);
        }

        /// <summary>
        /// Delete content by key (Admin only)
        /// </summary>
        [HttpDelete("{key}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteContent(string key)
        {
            var result = await _contentService.DeleteAsync(key);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Get content by category
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<Content>>> GetContentByCategory(string category)
        {
            var content = await _contentService.GetByCategoryAsync(category);
            return Ok(content);
        }
    }

    public class UpdateContentDto
    {
        public string Value { get; set; } = string.Empty;
    }
}