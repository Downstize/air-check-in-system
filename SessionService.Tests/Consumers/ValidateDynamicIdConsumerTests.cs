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
    public class ValidateDynamicIdConsumerTests
    {
        private ApplicationDbContext CreateDb(string dbName) =>
            new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options);

        [Fact]
        public async Task Consume_RespondsTrue_WhenExists()
        {
            var db = CreateDb(nameof(Consume_RespondsTrue_WhenExists));
            db.ActiveSessions.Add(new ActiveSession { DynamicId = "Z" });
            await db.SaveChangesAsync();
            var logger = Mock.Of<ILogger<ValidateDynamicIdConsumer>>();
            var consumer = new ValidateDynamicIdConsumer(db, logger);

            var request = new ValidateDynamicIdRequest { DynamicId = "Z" };
            var context = new Mock<ConsumeContext<ValidateDynamicIdRequest>>();
            context.SetupGet(x => x.Message).Returns(request);
            context.Setup(x => x.RespondAsync(It.IsAny<ValidateDynamicIdResponse>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await consumer.Consume(context.Object);

            context.Verify(x => x.RespondAsync(It.Is<ValidateDynamicIdResponse>(r => r.IsValid)), Times.Once);
        }

        [Fact]
        public async Task Consume_RespondsFalse_WhenNotExists()
        {
            var db = CreateDb(nameof(Consume_RespondsFalse_WhenNotExists));
            var logger = Mock.Of<ILogger<ValidateDynamicIdConsumer>>();
            var consumer = new ValidateDynamicIdConsumer(db, logger);

            var request = new ValidateDynamicIdRequest { DynamicId = "Q" };
            var context = new Mock<ConsumeContext<ValidateDynamicIdRequest>>();
            context.SetupGet(x => x.Message).Returns(request);
            context.Setup(x => x.RespondAsync(It.IsAny<ValidateDynamicIdResponse>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await consumer.Consume(context.Object);

            context.Verify(x => x.RespondAsync(It.Is<ValidateDynamicIdResponse>(r => !r.IsValid)), Times.Once);
        }
    }
}