using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnvironmentCreatorAPI.Data;
using EnvironmentCreatorAPI.Models;
using System.Security.Claims;

namespace EnvironmentCreatorAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WorldsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WorldsController> _logger;

        public WorldsController(ApplicationDbContext context, ILogger<WorldsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateWorld([FromBody] Environment2D world)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Invalid userId in token.");
            }


            if (string.IsNullOrWhiteSpace(world.Name) || world.Name.Length > 25)
                return BadRequest("Naam moet tussen 1 en 25 karakters zijn.");

            if (_context.Worlds.Any(w => w.UserId == userId && w.Name == world.Name))
                return BadRequest("Een wereld met deze naam bestaat al.");

            if (_context.Worlds.Count(w => w.UserId == userId) >= 5)
                return BadRequest("Je kunt maximaal 5 werelden hebben.");

            _context.Worlds.Add(world);
            _context.SaveChanges();

            return Ok(world);
        }


        [HttpGet]
        public IActionResult GetWorlds()
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

                var worlds = _context.Worlds.Where(w => w.UserId == userId).ToList();

                if (worlds == null || worlds.Count == 0)
                {
                    _logger.LogInformation("No worlds found for user: {UserId}", userId);
                    return NotFound("No worlds found.");
                }

                _logger.LogInformation("Successfully fetched {WorldCount} worlds for user: {UserId}", worlds.Count, userId);
                return Ok(worlds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching worlds for user.");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteWorld(int id)
        {
            var world = _context.Worlds.Find(id);
            if (world == null) return NotFound();

            _context.Worlds.Remove(world);
            _context.SaveChanges();
            return Ok("Wereld verwijderd.");
        }
    }
}