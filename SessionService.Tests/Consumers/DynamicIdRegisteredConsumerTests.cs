using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SessionService.Consumers;
using SessionService.Data;
using SessionService.Models;
using Shared.Messages;

namespace SessionService.Tests.Consumers
{
    public class DynamicIdRegisteredConsumerTests
    {
        private ApplicationDbContext CreateDb(string dbName) =>
            new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options);

        [Fact]
        public async Task Consume_AddsNewSession_WhenNotExists()
        {
            var db = CreateDb(nameof(Consume_AddsNewSession_WhenNotExists));
            var logger = Mock.Of<ILogger<DynamicIdRegisteredConsumer>>();
            var consumer = new DynamicIdRegisteredConsumer(db, logger);

            var message = new DynamicIdRegistered { DynamicId = "X", CreatedAt = System.DateTime.UtcNow };
            var context = new Mock<ConsumeContext<DynamicIdRegistered>>();
            context.SetupGet(x => x.Message).Returns(message);

            await consumer.Consume(context.Object);

            var saved = await db.ActiveSessions.FirstOrDefaultAsync(s => s.DynamicId == "X");
            Assert.NotNull(saved);
        }

        [Fact]
        public async Task Consume_DoesNotAdd_WhenAlreadyExists()
        {
            var db = CreateDb(nameof(Consume_DoesNotAdd_WhenAlreadyExists));
            db.ActiveSessions.Add(new ActiveSession { DynamicId = "Y" });
            await db.SaveChangesAsync();
            var logger = Mock.Of<ILogger<DynamicIdRegisteredConsumer>>();
            var consumer = new DynamicIdRegisteredConsumer(db, logger);

            var message = new DynamicIdRegistered { DynamicId = "Y", CreatedAt = System.DateTime.UtcNow };
            var context = new Mock<ConsumeContext<DynamicIdRegistered>>();
            context.SetupGet(x => x.Message).Returns(message);

            await consumer.Consume(context.Object);

            var count = await db.ActiveSessions.CountAsync(s => s.DynamicId == "Y");
            Assert.Equal(1, count);
        }
    }
}