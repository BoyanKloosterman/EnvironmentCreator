using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnvironmentCreatorAPI.Data;
using EnvironmentCreatorAPI.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EnvironmentCreatorAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EnvironmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EnvironmentController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public EnvironmentController(
            ApplicationDbContext context,
            ILogger<EnvironmentController> logger,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEnvironment([FromBody] Environment2D world)
        {
            // Get current user ID using Identity Framework
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not authenticated.");
            }

            // Store user ID in the environment
            world.UserId = user.Id;

            if (string.IsNullOrWhiteSpace(world.Name) || world.Name.Length > 25)
                return BadRequest("Naam moet tussen 1 en 25 karakters zijn.");

            if (_context.Environments.Any(w => w.UserId == user.Id && w.Name == world.Name))
                return BadRequest("Een wereld met deze naam bestaat al.");

            if (_context.Environments.Count(w => w.UserId == user.Id) >= 5)
                return BadRequest("Je kunt maximaal 5 werelden hebben.");

            _context.Environments.Add(world);
            await _context.SaveChangesAsync();

            return Ok(world);
        }

        [HttpGet]
        public async Task<IActionResult> GetEnvironments()
        {
            try
            {
                // Get current user ID using Identity Framework
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogError("User not authenticated.");
                    return Unauthorized("User not authenticated.");
                }

                var userId = user.Id;
                _logger.LogInformation("Fetching worlds for user: {UserId}", userId);

                var environments = await _context.Environments
                    .Where(w => w.UserId == userId)
                    .ToListAsync();

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
            // Get current user ID using Identity Framework
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var userId = user.Id;

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

            return Ok("Success");
        }
    }
}