using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnvironmentCreatorAPI.Data;
using EnvironmentCreatorAPI.Models;
using BCrypt.Net;
using System.Diagnostics;


namespace EnvironmentCreatorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, IConfiguration config, ILogger<AuthController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserDTO userDto)
        {
            if (_context.Users.Any(u => u.Username == userDto.Username))
                return BadRequest("Gebruikersnaam bestaat al.");

            var user = new User
            {
                Username = userDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("Registratie succesvol.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDTO loginDto)
        {
            try
            {
                if (loginDto == null)
                {
                    return BadRequest("Invalid request body.");
                }

                var user = _context.Users.FirstOrDefault(u => u.Username == loginDto.Username);
                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                    return Unauthorized("Invalid credentials.");

                var token = GenerateJwtToken(user);
                Debug.WriteLine(token);
                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt.");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(JwtRegisteredClaimNames.Aud, "http://localhost:5067"),
        new Claim(JwtRegisteredClaimNames.Iss, "EnvironmentCreatorAPI")
    };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}