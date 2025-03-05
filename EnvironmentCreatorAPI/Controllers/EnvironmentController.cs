using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnvironmentCreatorAPI.Data;
using EnvironmentCreatorAPI.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace EnvironmentCreatorAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EnvironmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EnvironmentController> _logger;

        public EnvironmentController(ApplicationDbContext context, ILogger<EnvironmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateEnvironment([FromBody] Environment2D world)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Invalid userId in token.");
            }


            if (string.IsNullOrWhiteSpace(world.Name) || world.Name.Length > 25)
                return BadRequest("Naam moet tussen 1 en 25 karakters zijn.");

            if (_context.Environments.Any(w => w.UserId == userId && w.Name == world.Name))
                return BadRequest("Een wereld met deze naam bestaat al.");

            if (_context.Environments.Count(w => w.UserId == userId) >= 5)
                return BadRequest("Je kunt maximaal 5 werelden hebben.");

            _context.Environments.Add(world);
            _context.SaveChanges();

            return Ok(world);
        }


        [HttpGet]
        public IActionResult GetEnvironments()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    _logger.LogError("No userId found in token.");
                    return Unauthorized("Token invalid or missing userId.");
                }

                var userId = int.Parse(userIdClaim.Value);
                _logger.LogInformation("Fetching worlds for user: {UserId}", userId);

                var environments = _context.Environments.Where(w => w.UserId == userId).ToList();

                if (environments == null || environments.Count == 0)
                {
                    _logger.LogInformation("No worlds found for user: {UserId}", userId);
                    return NotFound("No worlds found.");
                }

                _logger.LogInformation("Successfully fetched {WorldCount} worlds for user: {UserId}", environments.Count, userId);
                return Ok(environments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching worlds for user.");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnvironment(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Invalid userId in token.");
            }

            var environment = await _context.Environments
                .FirstOrDefaultAsync(e => e.EnvironmentId == id && e.UserId == userId);

            if (environment == null)
            {
                return NotFound("Environment not found or not owned by user.");
            }

            var objectsToDelete = _context.Objects.Where(o => o.EnvironmentId == id);
            _context.Objects.RemoveRange(objectsToDelete);

            _context.Environments.Remove(environment);

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}