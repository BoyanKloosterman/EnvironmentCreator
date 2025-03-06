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
using System.Threading.Tasks;

namespace EnvironmentCreatorAPI.Tests
{
    [TestClass]
    public class ObjectsControllerTests
    {
        private ObjectsController _controller = null!;
        private ApplicationDbContext _context = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase" + System.Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _controller = new ObjectsController(_context);

            // Seed the database with a test environment
            _context.Environments.Add(new Environment2D
            {
                EnvironmentId = 1,
                UserId = "1",
                Name = "TestEnvironment",
                MaxWidth = 100,
                MaxHeight = 100
            });
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task PostObject_ShouldReturnCreatedAtAction_WhenObjectIsCreatedSuccessfully()
        {
            var objectDto = new Object2DDto
            {
                EnvironmentId = 1,
                PrefabId = 1,
                PositionX = 10,
                PositionY = 20,
                ScaleX = 1,
                ScaleY = 1,
                RotationZ = 0,
                SortingLayer = 0
            };

            var result = await _controller.PostObject(objectDto);

            var createdAtActionResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtActionResult);
            Assert.AreEqual(201, createdAtActionResult.StatusCode);

            var createdObject = createdAtActionResult.Value as Object2D;
            Assert.IsNotNull(createdObject);
            Assert.AreEqual(objectDto.EnvironmentId, createdObject.EnvironmentId);
            Assert.AreEqual(objectDto.PrefabId, createdObject.PrefabId);
        }

        [TestMethod]
        public async Task PostObject_ShouldReturnBadRequest_WhenObjectDtoIsNull()
        {
            Object2DDto? objectDto = null;
            var result = await _controller.PostObject(objectDto!);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Object data is invalid.", badRequestResult.Value);
        }




        [TestMethod]
        public async Task PostObject_ShouldReturnNotFound_WhenEnvironmentDoesNotExist()
        {
            var objectDto = new Object2DDto
            {
                EnvironmentId = 999,
                PrefabId = 1,
                PositionX = 10,
                PositionY = 20,
                ScaleX = 1,
                ScaleY = 1,
                RotationZ = 0,
                SortingLayer = 0
            };

            var result = await _controller.PostObject(objectDto);

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Environment not found.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetObjectById_ShouldReturnObject_WhenObjectExists()
        {
            var object2D = new Object2D
            {
                EnvironmentId = 1,
                PrefabId = 1,
                PositionX = 10,
                PositionY = 20,
                ScaleX = 1,
                ScaleY = 1,
                RotationZ = 0,
                SortingLayer = 0
            };
            _context.Objects.Add(object2D);
            _context.SaveChanges();

            var result = await _controller.GetObjectById(object2D.ObjectId);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnedObject = okResult.Value as Object2D;
            Assert.IsNotNull(returnedObject);
            Assert.AreEqual(object2D.ObjectId, returnedObject.ObjectId);
        }

        [TestMethod]
        public async Task GetObjectById_ShouldReturnNotFound_WhenObjectDoesNotExist()
        {
            var result = await _controller.GetObjectById(999);

            var notFoundResult = result.Result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task GetObjectsByEnvironment_ShouldReturnObjects_WhenObjectsExist()
        {
            var object2D = new Object2D
            {
                EnvironmentId = 1,
                PrefabId = 1,
                PositionX = 10,
                PositionY = 20,
                ScaleX = 1,
                ScaleY = 1,
                RotationZ = 0,
                SortingLayer = 0
            };
            _context.Objects.Add(object2D);
            _context.SaveChanges();

            var result = await _controller.GetObjectsByEnvironment(1);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var objects = okResult.Value as List<Object2D>;
            Assert.IsNotNull(objects);
            Assert.AreEqual(1, objects.Count);
        }

        [TestMethod]
        public async Task GetObjectsByEnvironment_ShouldReturnNotFound_WhenNoObjectsExist()
        {
            var result = await _controller.GetObjectsByEnvironment(1);

            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("No objects found for this environment.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task GetObjects_ShouldReturnAllObjects()
        {
            var object2D = new Object2D
            {
                EnvironmentId = 1,
                PrefabId = 1,
                PositionX = 10,
                PositionY = 20,
                ScaleX = 1,
                ScaleY = 1,
                RotationZ = 0,
                SortingLayer = 0
            };
            _context.Objects.Add(object2D);
            _context.SaveChanges();

            var result = await _controller.GetObjects();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var objects = okResult.Value as List<Object2D>;
            Assert.IsNotNull(objects);
            Assert.AreEqual(1, objects.Count);
        }
    }
}
