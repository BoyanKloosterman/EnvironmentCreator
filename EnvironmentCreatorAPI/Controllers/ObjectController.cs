using Microsoft.AspNetCore.Mvc;
using EnvironmentCreatorAPI.Models;
using System.Threading.Tasks;
using EnvironmentCreatorAPI.Data;

namespace EnvironmentCreatorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ObjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostObject(Object2D object2D)
        {
            if (object2D == null)
            {
                return BadRequest("Object data is invalid.");
            }

            var environment = await _context.Worlds.FindAsync(object2D.EnvironmentId);
            if (environment == null)
            {
                return NotFound("Environment not found.");
            }

            _context.Objects.Add(object2D);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetObjectById), new { id = object2D.ObjectId }, object2D);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Object2D>> GetObjectById(int id)
        {
            var object2D = await _context.Objects.FindAsync(id);

            if (object2D == null)
            {
                return NotFound();
            }

            return object2D;
        }
    }
}
