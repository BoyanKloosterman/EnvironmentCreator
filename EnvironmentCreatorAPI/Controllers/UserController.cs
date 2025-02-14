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
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, IConfiguration config, ILogger<UserController> logger)
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

                var loginResponse = new LoginResponse
                {
                    Token = token,
                    UserId = user.UserId
                };

                return Ok(loginResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt.");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        public class LoginResponse
        {
            public string Token { get; set; }
            public int UserId { get; set; }
        }


        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
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