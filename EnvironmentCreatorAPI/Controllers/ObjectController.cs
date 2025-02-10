using Microsoft.AspNetCore.Mvc;
using EnvironmentCreatorAPI.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        // POST object to environment
        [HttpPost]
        public async Task<IActionResult> PostObject([FromBody] Object2DDto objectDto)
        {
            if (objectDto == null)
            {
                return BadRequest("Object data is invalid.");
            }

            var environment = await _context.Environments.FindAsync(objectDto.EnvironmentId);
            if (environment == null)
            {
                return NotFound("Environment not found.");
            }

            var object2D = new Object2D
            {
                EnvironmentId = objectDto.EnvironmentId,
                PrefabId = objectDto.PrefabId,
                PositionX = objectDto.PositionX,
                PositionY = objectDto.PositionY,
                ScaleX = objectDto.ScaleX,
                ScaleY = objectDto.ScaleY,
                RotationZ = objectDto.RotationZ,
                SortingLayer = objectDto.SortingLayer
            };

            _context.Objects.Add(object2D);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetObjectById), new { id = object2D.ObjectId }, object2D);
        }

        // GET object by ID
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

        // GET all objects for a specific environment
        [HttpGet("environment/{environmentId}")]
        public async Task<ActionResult<IEnumerable<Object2D>>> GetObjectsByEnvironment(int environmentId)
        {
            var objects = await _context.Objects
                                         .Where(o => o.EnvironmentId == environmentId)
                                         .ToListAsync();

            if (objects == null || objects.Count == 0)
            {
                return NotFound($"No objects found for environment with ID {environmentId}.");
            }

            return objects;
        }

        [HttpGet]
        public async Task<IActionResult> GetObjects()
        {
            var objects = await _context.Objects.ToListAsync();
            return Ok(objects);
        }

    }
}
