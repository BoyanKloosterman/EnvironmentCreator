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
        public async Task<IActionResult> PostObject([FromBody] Object2DDto objectDto)
        {
            if (objectDto == null)
            {
                return BadRequest("Object data is invalid.");
            }

            var environment = await _context.Worlds.FindAsync(objectDto.EnvironmentId);
            if (environment == null)
            {
                return NotFound("Environment not found.");
            }

            var object2D = new Object2D
            {
                EnvironmentId = objectDto.EnvironmentId, // Assign only the ID
                PrefabId = objectDto.PrefabId,
                PositionX = objectDto.PositionX,
                PositionY = objectDto.PositionY,
                ScaleX = objectDto.ScaleX,
                ScaleY = objectDto.ScaleY,
                RotationZ = objectDto.RotationZ,
                SortingLayer = objectDto.SortingLayer
            };

            _context.Object2D.Add(object2D);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetObjectById), new { id = object2D.ObjectId }, object2D);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Object2D>> GetObjectById(int id)
        {
            var object2D = await _context.Object2D.FindAsync(id);

            if (object2D == null)
            {
                return NotFound();
            }

            return object2D;
        }
    }
}
