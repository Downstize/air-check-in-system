using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SessionService.Data;
using SessionService.Models;
using SessionService.Services;

namespace SessionService.Tests.Services
{
    public class SessionsServiceTests
    {
        private ApplicationDbContext CreateDb(string name) =>
            new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(name)
                .Options);

        private IDistributedCache CreateCache() =>
            new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

        [Fact]
        public async Task ValidateSessionAsync_ReturnsFalse_WhenAbsent()
        {
            var db = CreateDb(nameof(ValidateSessionAsync_ReturnsFalse_WhenAbsent));
            var svc = new SessionsService(db, NullLogger<SessionsService>.Instance, CreateCache());
            var result = await svc.ValidateSessionAsync("X");
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateSessionAsync_ReturnsTrue_WhenPresent()
        {
            var db = CreateDb(nameof(ValidateSessionAsync_ReturnsTrue_WhenPresent));
            db.ActiveSessions.Add(new ActiveSession { DynamicId = "Y" });
            await db.SaveChangesAsync();
            var svc = new SessionsService(db, NullLogger<SessionsService>.Instance, CreateCache());
            var result = await svc.ValidateSessionAsync("Y");
            Assert.True(result);
        }

        [Fact]
        public async Task RegisterSessionAsync_Adds_WhenNew()
        {
            var db = CreateDb(nameof(RegisterSessionAsync_Adds_WhenNew));
            var svc = new SessionsService(db, NullLogger<SessionsService>.Instance, CreateCache());
            await svc.RegisterSessionAsync("Z");
            var exists = await db.ActiveSessions.AnyAsync(s => s.DynamicId == "Z");
            Assert.True(exists);
        }

        [Fact]
        public async Task RegisterSessionAsync_DoesNotAdd_WhenExists()
        {
            var db = CreateDb(nameof(RegisterSessionAsync_DoesNotAdd_WhenExists));
            db.ActiveSessions.Add(new ActiveSession { DynamicId = "W" });
            await db.SaveChangesAsync();
            var svc = new SessionsService(db, NullLogger<SessionsService>.Instance, CreateCache());
            await svc.RegisterSessionAsync("W");
            var count = await db.ActiveSessions.CountAsync(s => s.DynamicId == "W");
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task GetAllAsync_CachesResults()
        {
            var db = CreateDb(nameof(GetAllAsync_CachesResults));
            var cache = CreateCache();
            var sessions = new List<ActiveSession>
            {
                new() { DynamicId = "A1" }
            };
            db.ActiveSessions.AddRange(sessions);
            await db.SaveChangesAsync();
            var svc = new SessionsService(db, NullLogger<SessionsService>.Instance, cache);
            var first = (await svc.GetAllAsync()).ToList();
            var payload = cache.GetString("admin:sessions");
            var second = JsonSerializer.Deserialize<IEnumerable<ActiveSession>>(payload);
            Assert.Single(first);
            Assert.Single(second);
        }
    }
}