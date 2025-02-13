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

namespace EnvironmentCreatorAPI.Tests
{
    [TestClass]
    public class EnvironmentControllerTests
    {
        private Mock<ILogger<EnvironmentController>> _mockLogger;
        private EnvironmentController _controller;
        private ApplicationDbContext _context;

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
            _controller = new EnvironmentController(_context, _mockLogger.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [TestMethod]
        public void CreateWorld_ShouldReturnOk_WhenWorldIsCreatedSuccessfully()
        {
            var world = new Environment2D
            {
                UserId = 1,
                Name = "NewWorld",
                MaxWidth = 100,
                MaxHeight = 100
            };

            var result = _controller.CreateWorld(world);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(world, okResult.Value);

            var createdWorld = _context.Environments.FirstOrDefault(w => w.Name == "NewWorld");
            Assert.IsNotNull(createdWorld);
        }

        [TestMethod]
        public void CreateWorld_ShouldReturnBadRequest_WhenWorldNameIsInvalid()
        {
            var world = new Environment2D
            {
                UserId = 1,
                Name = "",
                MaxWidth = 100,
                MaxHeight = 100
            };

            var result = _controller.CreateWorld(world);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Naam moet tussen 1 en 25 karakters zijn.", badRequestResult.Value);
        }

        [TestMethod]
        public void CreateWorld_ShouldReturnBadRequest_WhenWorldNameAlreadyExists()
        {
            _context.Environments.Add(new Environment2D
            {
                UserId = 1,
                Name = "ExistingWorld",
                MaxWidth = 100,
                MaxHeight = 100
            });
            _context.SaveChanges();

            var world = new Environment2D
            {
                UserId = 1,
                Name = "ExistingWorld",
                MaxWidth = 100,
                MaxHeight = 100
            };

            var result = _controller.CreateWorld(world);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Een wereld met deze naam bestaat al.", badRequestResult.Value);
        }

        [TestMethod]
        public void CreateWorld_ShouldReturnBadRequest_WhenUserHasMaxWorlds()
        {
            for (int i = 0; i < 5; i++)
            {
                _context.Environments.Add(new Environment2D
                {
                    UserId = 1,
                    Name = "World" + i,
                    MaxWidth = 100,
                    MaxHeight = 100
                });
            }
            _context.SaveChanges();

            var world = new Environment2D
            {
                UserId = 1,
                Name = "NewWorld",
                MaxWidth = 100,
                MaxHeight = 100
            };

            var result = _controller.CreateWorld(world);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Je kunt maximaal 5 werelden hebben.", badRequestResult.Value);
        }

        [TestMethod]
        public void GetWorlds_ShouldReturnOk_WhenWorldsExist()
        {
            _context.Environments.Add(new Environment2D
            {
                UserId = 1,
                Name = "World1",
                MaxWidth = 100,
                MaxHeight = 100
            });
            _context.SaveChanges();

            var result = _controller.GetWorlds();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var environments = okResult.Value as List<Environment2D>;
            Assert.IsNotNull(environments);
            Assert.AreEqual(1, environments.Count);
        }

        [TestMethod]
        public void GetWorlds_ShouldReturnNotFound_WhenNoWorldsExist()
        {
            var result = _controller.GetWorlds();

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("No worlds found.", notFoundResult.Value);
        }

        [TestMethod]
        public void DeleteWorld_ShouldReturnOk_WhenWorldIsDeletedSuccessfully()
        {
            var world = new Environment2D
            {
                UserId = 1,
                Name = "WorldToDelete",
                MaxWidth = 100,
                MaxHeight = 100
            };
            _context.Environments.Add(world);
            _context.SaveChanges();

            var result = _controller.DeleteWorld(world.EnvironmentId);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Wereld verwijderd.", okResult.Value);

            var deletedWorld = _context.Environments.Find(world.EnvironmentId);
            Assert.IsNull(deletedWorld);
        }

        [TestMethod]
        public void DeleteWorld_ShouldReturnNotFound_WhenWorldDoesNotExist()
        {
            var result = _controller.DeleteWorld(999);

            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
    }
}
