using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration; // Correct IConfiguration import
using EnvironmentCreatorAPI.Controllers;
using EnvironmentCreatorAPI.Data;
using EnvironmentCreatorAPI.Models;
using BCrypt.Net;
using System.Linq;

namespace EnvironmentCreatorAPI.Tests
{
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<ApplicationDbContext> _mockContext;
        private Mock<IConfiguration> _mockConfig;
        private Mock<ILogger<AuthController>> _mockLogger;
        private AuthController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<ApplicationDbContext>();
            _mockConfig = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthController>>();

            // Mocking IConfiguration for the JWT secret key
            _mockConfig.Setup(config => config["JwtSettings:SecretKey"]).Returns("YourSecretKey");

            _controller = new AuthController(_mockContext.Object, _mockConfig.Object, _mockLogger.Object);
        }

        [TestMethod]
        public void Register_ShouldReturnOk_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange
            var userDto = new UserDTO { Username = "newUser", Password = "newPassword" };

            _mockContext.Setup(c => c.Users.Any(u => u.Username == userDto.Username)).Returns(false); // User doesn't exist
            _mockContext.Setup(c => c.Users.Add(It.IsAny<User>())).Verifiable();
            _mockContext.Setup(c => c.SaveChanges()).Verifiable();

            // Act
            var result = _controller.Register(userDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Registratie succesvol.", okResult.Value);

            _mockContext.Verify(c => c.Users.Add(It.IsAny<User>()), Times.Once); // Verifies if Add was called once
            _mockContext.Verify(c => c.SaveChanges(), Times.Once); // Verifies if SaveChanges was called once
        }

        [TestMethod]
        public void Register_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
        {
            // Arrange
            var userDto = new UserDTO { Username = "existingUser", Password = "password" };

            _mockContext.Setup(c => c.Users.Any(u => u.Username == userDto.Username)).Returns(true); // User already exists

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
            var user = new User
            {
                UserId = 1,
                Username = "validUser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("validPassword")
            };

            _mockContext.Setup(c => c.Users.FirstOrDefault(u => u.Username == loginDto.Username)).Returns(user); // Valid user found
            _mockContext.Setup(c => c.Users.Any(u => u.Username == loginDto.Username)).Returns(true);

            // Act
            var result = _controller.Login(loginDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsTrue(okResult.Value.ToString().StartsWith("eyJ")); // JWT token should be returned
        }

        [TestMethod]
        public void Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginDto = new UserDTO { Username = "invalidUser", Password = "invalidPassword" };

            _mockContext.Setup(c => c.Users.FirstOrDefault(u => u.Username == loginDto.Username)).Returns((User)null); // User not found

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
