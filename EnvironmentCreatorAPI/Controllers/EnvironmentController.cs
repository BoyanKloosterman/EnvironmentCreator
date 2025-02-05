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
        private readonly ILogger _logger;

        public WorldsController(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateWorld([FromBody] Environment2D world)
        {
            var userId = int.Parse(User.FindFirst("nameidentifier").Value);
            world.UserId = userId;

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
                // Haal de userId uit de claims van de JWT-token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("Gebruiker niet geauthenticeerd.");
                }

                int userId = int.Parse(userIdClaim.Value);

                // Haal werelden op die overeenkomen met de gebruiker
                var worlds = _context.Worlds.Where(w => w.UserId == userId).ToList();

                if (worlds == null || worlds.Count == 0)
                {
                    return NotFound("Geen werelden gevonden.");
                }

                return Ok(worlds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van werelden");
                return StatusCode(500, "Er is een interne serverfout opgetreden.");
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