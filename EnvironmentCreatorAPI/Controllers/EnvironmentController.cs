using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnvironmentCreatorAPI.Data;
using EnvironmentCreatorAPI.Models;

namespace EnvironmentCreatorAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WorldsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WorldsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateWorld([FromBody] Environment2D world)
        {
            var userId = int.Parse(User.FindFirst("nameidentifier").Value);
            world.UserId = userId;

            _context.Worlds.Add(world);
            _context.SaveChanges();

            return Ok(world);
        }

        [HttpGet]
        public IActionResult GetWorlds()
        {
            var userId = int.Parse(User.FindFirst("nameidentifier").Value);
            var worlds = _context.Worlds.Where(w => w.UserId == userId).ToList();
            return Ok(worlds);
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