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
        private Mock<IConfiguration> _mockConfig;
        private Mock<ILogger<AuthController>> _mockLogger;
        private AuthController _controller;
        private ApplicationDbContext _context;

        [TestInitialize]
        public void Setup()
        {
            // Set up an in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase" + Guid.NewGuid()) // Unique name for each test
                .Options;

            _context = new ApplicationDbContext(options);

            // Clear any existing users to ensure a clean state
            _context.Database.EnsureDeleted(); // Ensures the database is emptied
            _context.Database.EnsureCreated(); // Recreates the database

            // Seed the database with a test user
            _context.Users.Add(new User
            {
                UserId = 1, // Ensure this UserId does not conflict with others
                Username = "validUser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("validPassword")
            });
            _context.SaveChanges();

            // Mocking IConfiguration for JWT secret key
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(config => config["JwtSettings:SecretKey"]).Returns("YourSecretKey");

            // Mocking ILogger for AuthController
            _mockLogger = new Mock<ILogger<AuthController>>();

            // Initialize the controller with the in-memory database context
            _controller = new AuthController(_context, _mockConfig.Object, _mockLogger.Object);
        }


        [TestMethod]
        public void Register_ShouldReturnOk_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange
            var userDto = new UserDTO { Username = "newUser", Password = "newPassword" };

            // Ensure the user does not already exist in the context
            var userExists = _context.Users.Any(u => u.Username == userDto.Username);
            Assert.IsFalse(userExists, "User should not already exist.");

            // Act
            var result = _controller.Register(userDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Registratie succesvol.", okResult.Value);
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
        }

        [TestMethod]
        public void Login_ShouldReturnOk_WhenUserCredentialsAreValid()
        {
            // Arrange
            var loginDto = new UserDTO { Username = "validUser", Password = "validPassword" };

            // Ensure the user exists
            var user = _context.Users.FirstOrDefault(u => u.Username == loginDto.Username);
            Assert.IsNotNull(user, "User should exist in the database");

            // Act
            var result = _controller.Login(loginDto);

            // Log the result for debugging
            Console.WriteLine($"Result: {result.GetType().Name}"); // Debugging line

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "Result should be an OkObjectResult");
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsTrue(okResult.Value.ToString().StartsWith("eyJ"), "JWT token should start with 'eyJ'");
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
        }

        [TestMethod]
        public void Login_ShouldReturnBadRequest_WhenRequestBodyIsInvalid()
        {
            // Act
            var result = _controller.Login(null); // Passing null to simulate invalid body

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid request body.", badRequestResult.Value);
        }
    }
}
