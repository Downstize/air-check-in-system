using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SessionService.Controllers;
using SessionService.Models;
using SessionService.Services;

namespace SessionService.Tests.Controllers
{
    public class AdminSessionControllerTests
    {
        private readonly Mock<ISessionsService> _svc = new();
        private readonly AdminSessionController _ctrl;

        public AdminSessionControllerTests()
        {
            var logger = Mock.Of<ILogger<AdminSessionController>>();
            _ctrl = new AdminSessionController(_svc.Object, logger);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithList()
        {
            var list = new List<ActiveSession> { new() { Id = 1 } };
            _svc.Setup(x => x.GetAllAsync()).ReturnsAsync(list);
            var result = await _ctrl.GetAll();
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(list, ok.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNull()
        {
            _svc.Setup(x => x.GetByIdAsync(42)).ReturnsAsync((ActiveSession)null);
            var result = await _ctrl.GetById(42);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            var session = new ActiveSession { Id = 2 };
            _svc.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(session);
            var result = await _ctrl.GetById(2);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(session, ok.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAt_WithEntity()
        {
            var session = new ActiveSession { DynamicId = "A" };
            var created = new ActiveSession { Id = 3, DynamicId = "A" };
            _svc.Setup(x => x.CreateAsync(session)).ReturnsAsync(created);
            var result = await _ctrl.Create(session);
            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdAt.Value);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenUpdated()
        {
            var session = new ActiveSession { DynamicId = "B" };
            _svc.Setup(x => x.UpdateAsync(5, session)).ReturnsAsync(session);
            var result = await _ctrl.Update(5, session);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenMissing()
        {
            _svc.Setup(x => x.UpdateAsync(6, It.IsAny<ActiveSession>())).ReturnsAsync((ActiveSession)null);
            var result = await _ctrl.Update(6, new ActiveSession());
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeleted()
        {
            _svc.Setup(x => x.DeleteAsync(7)).ReturnsAsync(true);
            var result = await _ctrl.Delete(7);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            _svc.Setup(x => x.DeleteAsync(8)).ReturnsAsync(false);
            var result = await _ctrl.Delete(8);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}