using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnvironmentCreatorAPI.Controllers;
using EnvironmentCreatorAPI.Data;
using EnvironmentCreatorAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace EnvironmentCreatorAPI.Tests
{
    [TestClass]
    public class EnvironmentControllerTests
    {
        private Mock<ILogger<EnvironmentController>> _mockLogger = new();
        private Mock<UserManager<IdentityUser>> _mockUserManager = new(
            new Mock<IUserStore<IdentityUser>>().Object, null, null, null, null, null, null, null, null);
        private EnvironmentController _controller = null!;
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

            _mockLogger = new Mock<ILogger<EnvironmentController>>();
            _controller = new EnvironmentController(_context, _mockLogger.Object, _mockUserManager.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Setup UserManager to return a user when GetUserAsync is called
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new IdentityUser { Id = "1", UserName = "testuser" });
        }

        [TestMethod]
        public async Task CreateWorld_ShouldReturnOk_WhenWorldIsCreatedSuccessfully()
        {
            var world = new Environment2D
            {
                UserId = "1",
                Name = "NewWorld",
                MaxWidth = 100,
                MaxHeight = 100
            };

            var result = await _controller.CreateEnvironment(world);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(world, okResult.Value);

            var createdWorld = _context.Environments.FirstOrDefault(w => w.Name == "NewWorld");
            Assert.IsNotNull(createdWorld);
        }

        [TestMethod]
        public async Task CreateWorld_ShouldReturnBadRequest_WhenWorldNameIsInvalid()
        {
            var world = new Environment2D
            {
                UserId = "1",
                Name = "",
                MaxWidth = 100,
                MaxHeight = 100
            };

            var result = await _controller.CreateEnvironment(world);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Naam moet tussen 1 en 25 karakters zijn.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task CreateWorld_ShouldReturnBadRequest_WhenWorldNameAlreadyExists()
        {
            _context.Environments.Add(new Environment2D
            {
                UserId = "1",
                Name = "ExistingWorld",
                MaxWidth = 100,
                MaxHeight = 100
            });
            _context.SaveChanges();

            var world = new Environment2D
            {
                UserId = "1",
                Name = "ExistingWorld",
                MaxWidth = 100,
                MaxHeight = 100
            };

            var result = await _controller.CreateEnvironment(world);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Een wereld met deze naam bestaat al.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task CreateWorld_ShouldReturnBadRequest_WhenUserHasMaxWorlds()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                _context.Environments.Add(new Environment2D
                {
                    UserId = "1",
                    Name = "World" + i,
                    MaxWidth = 100,
                    MaxHeight = 100
                });
            }
            _context.SaveChanges();

            var world = new Environment2D
            {
                UserId = "1",
                Name = "NewWorld",
                MaxWidth = 100,
                MaxHeight = 100
            };

            // Act
            var result = await _controller.CreateEnvironment(world);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Je kunt maximaal 5 werelden hebben.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task GetWorlds_ShouldReturnOk_WhenWorldsExist()
        {
            _context.Environments.Add(new Environment2D
            {
                UserId = "1",
                Name = "World1",
                MaxWidth = 100,
                MaxHeight = 100
            });
            _context.SaveChanges();

            var result = await _controller.GetEnvironments();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var environments = okResult.Value as List<Environment2D>;
            Assert.IsNotNull(environments);
            Assert.AreEqual(1, environments.Count);
        }

        [TestMethod]
        public async Task GetWorlds_ShouldReturnNotFound_WhenNoWorldsExist()
        {
            var result = await _controller.GetEnvironments();

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("No worlds found.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task DeleteWorld_ShouldReturnSuccess_WhenWorldIsDeletedSuccessfully()
        {
            // Arrange
            var world = new Environment2D
            {
                UserId = "1",
                Name = "WorldToDelete",
                MaxWidth = 100,
                MaxHeight = 100
            };
            _context.Environments.Add(world);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteEnvironment(world.EnvironmentId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task DeleteWorld_ShouldReturnNotFound_WhenWorldDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteEnvironment(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }
    }
}