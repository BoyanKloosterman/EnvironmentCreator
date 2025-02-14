using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using EnvironmentCreatorAPI.Controllers;
using EnvironmentCreatorAPI.Data;
using EnvironmentCreatorAPI.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace EnvironmentCreatorAPI.Tests
{
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<IConfiguration> _mockConfig = null!;
        private Mock<ILogger<UserController>> _mockLogger = null!;
        private UserController _controller = null!;
        private ApplicationDbContext _context = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase" + Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);


            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _context.Users.Add(new User
            {
                UserId = 1,
                Username = "validUser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("validPassword")
            });
            _context.SaveChanges();

            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(config => config["JwtSettings:SecretKey"]).Returns(
            "ThisIsASecretKeyThatIsAtLeast32BytesLongAndSecure"
            );


            _mockLogger = new Mock<ILogger<UserController>>();

            _controller = new UserController(_context, _mockConfig.Object, _mockLogger.Object);
        }

        [TestMethod]
        public void Register_ShouldReturnOk_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange
            var userDto = new UserDTO { Username = "newUser", Password = "newPassword" };

            var userExists = _context.Users.Any(u => u.Username == userDto.Username);
            Assert.IsFalse(userExists, "User should not already exist.");

            // Act
            var result = _controller.Register(userDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Registratie succesvol.", okResult.Value);

            // Additional criteria
            var newUser = _context.Users.FirstOrDefault(u => u.Username == userDto.Username);
            Assert.IsNotNull(newUser, "New user should be added to the database.");
            Assert.IsTrue(BCrypt.Net.BCrypt.Verify("newPassword", newUser.PasswordHash), "Password should be hashed correctly.");
        }

        [TestMethod]
        public void Register_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
        {
            // Arrange
            var userDto = new UserDTO { Username = "validUser", Password = "password" };

            // Ensure the user exists in the context
            var userExists = _context.Users.Any(u => u.Username == userDto.Username);
            Assert.IsTrue(userExists, "User should already exist.");

            // Act
            var result = _controller.Register(userDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Gebruikersnaam bestaat al.", badRequestResult.Value);

            // Additional criteria
            var userCount = _context.Users.Count(u => u.Username == userDto.Username);
            Assert.AreEqual(1, userCount, "There should still be only one user with the same username.");
        }

        [TestMethod]
        public void Login_ShouldReturnOk_WhenUserCredentialsAreValid()
        {
            // Arrange
            var username = "validUser";
            var password = "validPassword";

            // Ensure user exists with a properly hashed password
            if (!_context.Users.Any(u => u.Username == username))
            {
                var user = new User
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password) // Hash the password
                };
                _context.Users.Add(user);
                _context.SaveChanges();
            }

            var loginDto = new UserDTO { Username = username, Password = password };

            // Ensure user exists before attempting login
            var userInDb = _context.Users.FirstOrDefault(u => u.Username == username);
            Assert.IsNotNull(userInDb, "User should exist in the database");
            Console.WriteLine($"User found: {userInDb.Username}, Hash: {userInDb.PasswordHash}");

            // Act
            var result = _controller.Login(loginDto);

            // Ensure result is not null
            Assert.IsNotNull(result, "Login result should not be null");

            // Log result type
            Console.WriteLine($"Actual result type: {result.GetType().Name}");

            if (result is ObjectResult objectResult)
            {
                Console.WriteLine($"ObjectResult Status Code: {objectResult.StatusCode}");
                Console.WriteLine($"ObjectResult Value: {objectResult.Value}");
            }

            // Assert that result is an OkObjectResult
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, $"Expected OkObjectResult but got {result.GetType().Name}");

            // Ensure status code is 200
            Assert.AreEqual(200, okResult.StatusCode, "Expected status code 200");

            // Ensure the response contains a valid JWT token
            var loginResponse = okResult.Value as UserController.LoginResponse;
            Assert.IsNotNull(loginResponse, "Expected LoginResponse object");
            Assert.IsFalse(string.IsNullOrEmpty(loginResponse.Token), "JWT token should not be null or empty");
            Assert.IsTrue(loginResponse.Token.StartsWith("eyJ"), "JWT token should start with 'eyJ'");

            Console.WriteLine($"JWT Token: {loginResponse.Token}");
        }


        [TestMethod]
        public void Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginDto = new UserDTO { Username = "invalidUser", Password = "invalidPassword" };

            // Act
            var result = _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
            Assert.AreEqual("Invalid credentials.", unauthorizedResult.Value);

            // Additional criteria
            var user = _context.Users.FirstOrDefault(u => u.Username == loginDto.Username);
            Assert.IsNull(user, "User should not exist in the database.");
        }
        [TestMethod]
        public void Login_ShouldReturnBadRequest_WhenRequestBodyIsInvalid()
        {
            // Act
            var result = _controller.Login(new UserDTO());

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid request body.", badRequestResult.Value);

            // Additional criteria
            var userCount = _context.Users.Count();
            Assert.AreEqual(1, userCount, "User count should remain unchanged.");
        }
    }
}
