using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using PassengerService.Services;
using PassengerService.Tests.Utilities;
using Shared.Contracts;

namespace PassengerService.Tests.Services
{
    public class PassengersServiceTests
    {
        private readonly PassengersService _service;
        private readonly Mock<IDistributedCache> _cacheMock;

        public PassengersServiceTests()
        {
            var db = TestDbContextFactory.CreateInMemory();
            _cacheMock = new Mock<IDistributedCache>();
            _cacheMock
                .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));
            _cacheMock
                .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null);
            var logger = Mock.Of<ILogger<PassengersService>>();
            _service = new PassengersService(db, logger, _cacheMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_CacheHit_ReturnsCachedList()
        {
            var list = new List<PassengerDto>
            {
                new PassengerDto
                {
                    PassengerId = "X1",
                    PnrId = "P1",
                    PaxNo = 1,
                    LastName = "L",
                    FirstName = "F",
                    BirthDate = DateTime.UtcNow.AddYears(-30),
                    Category = "ADT",
                    CheckInStatus = "none",
                    SeatsOccupied = 1,
                    Eticket = true,
                    Remarks = new List<string>(),
                    Apis = 0,
                    BookingId = 1,
                    SeatNumber = "1A",
                    SeatStatus = "free",
                    SeatLayerType = "economy",
                    Document = new PassengerDocumentDto
                    {
                        Type = "passport",
                        IssueCountryCode = "RU",
                        Number = "X",
                        NationalityCode = "RU",
                        BirthDate = DateTime.UtcNow.AddYears(-30),
                        ExpiryDate = DateTime.UtcNow.AddYears(5)
                    },
                    VisaDocument = new PassengerVisaDocumentDto
                    {
                        BirthPlace = "X",
                        Number = "V",
                        IssuePlace = "Y",
                        IssueDate = DateTime.UtcNow.AddYears(-1),
                        ApplicCountryCode = "US"
                    }
                }
            };
            var json = JsonSerializer.Serialize(list);
            var bytes = Encoding.UTF8.GetBytes(json);
            _cacheMock
                .Setup(c => c.GetAsync("passenger:all", It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);

            var result = await _service.GetAllAsync();

            var single = Assert.Single(result);
            Assert.Equal("X1", single.PassengerId);
        }

        [Fact]
        public async Task GetAllAsync_CacheMiss_CachesResult()
        {
            var result = await _service.GetAllAsync();

            Assert.NotEmpty(result);
            _cacheMock.Verify(c => c.SetAsync(
                    "passenger:all",
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    default),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_AddsEntityAndClearsCache()
        {
            var dto = new PassengerDto
            {
                PassengerId = Guid.NewGuid().ToString(),
                BookingId = 1,
                PnrId = "X",
                PaxNo = 1,
                LastName = "L",
                FirstName = "F",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                Category = "ADT",
                CheckInStatus = "none",
                SeatsOccupied = 1,
                Eticket = false,
                Remarks = new List<string>(),
                Apis = 0,
                SeatNumber = "1A",
                SeatStatus = "free",
                SeatLayerType = "econ",
                Document = new PassengerDocumentDto
                {
                    Type = "passport",
                    IssueCountryCode = "RU",
                    Number = "D1",
                    NationalityCode = "RU",
                    BirthDate = DateTime.UtcNow.AddYears(-30),
                    ExpiryDate = DateTime.UtcNow.AddYears(5)
                },
                VisaDocument = new PassengerVisaDocumentDto
                {
                    BirthPlace = "X",
                    Number = "V1",
                    IssuePlace = "Y",
                    IssueDate = DateTime.UtcNow.AddYears(-1),
                    ApplicCountryCode = "US"
                }
            };

            var created = await _service.CreateAsync(dto);

            var expectedJson = JsonSerializer.Serialize(dto);
            var actualJson = JsonSerializer.Serialize(created);
            Assert.Equal(expectedJson, actualJson);
            _cacheMock.Verify(c => c.RemoveAsync("passenger:all", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            var result = await _service.GetByIdAsync("NOPE");
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_NotFound_ReturnsFalse()
        {
            var result = await _service.UpdateAsync("NOPE", new PassengerDto());
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_Valid_UpdatesAndClearsCache()
        {
            var db = TestDbContextFactory.CreateInMemory();
            var service = new PassengersService(db, Mock.Of<ILogger<PassengersService>>(), _cacheMock.Object);

            var existing = db.Passengers.First();
            var dto = new PassengerDto
            {
                PassengerId = existing.PassengerId,
                BookingId = existing.BookingId,
                PnrId = existing.PnrId,
                PaxNo = existing.PaxNo,
                LastName = "NEW",
                FirstName = existing.FirstName,
                BirthDate = existing.BirthDate,
                Category = existing.Category,
                CheckInStatus = existing.CheckInStatus,
                SeatsOccupied = existing.SeatsOccupied,
                Eticket = existing.Eticket,
                SeatNumber = existing.SeatNumber,
                SeatStatus = existing.SeatStatus,
                SeatLayerType = existing.SeatLayerType,
                Remarks = existing.Remarks,
                Apis = existing.Apis,
                Document = new PassengerDocumentDto
                {
                    Type = existing.Document.Type,
                    IssueCountryCode = existing.Document.IssueCountryCode,
                    Number = existing.Document.Number,
                    NationalityCode = existing.Document.NationalityCode,
                    BirthDate = existing.Document.BirthDate,
                    ExpiryDate = existing.Document.ExpiryDate
                },
                VisaDocument = new PassengerVisaDocumentDto
                {
                    BirthPlace = existing.VisaDocument.BirthPlace,
                    Number = existing.VisaDocument.Number,
                    IssuePlace = existing.VisaDocument.IssuePlace,
                    IssueDate = existing.VisaDocument.IssueDate,
                    ApplicCountryCode = existing.VisaDocument.ApplicCountryCode
                }
            };

            var updated = await service.UpdateAsync(existing.PassengerId, dto);

            Assert.True(updated);
            var reloaded = db.Passengers.Find(existing.PassengerId);
            Assert.Equal("NEW", reloaded!.LastName);
            _cacheMock.Verify(c => c.RemoveAsync($"passenger:{existing.PassengerId}", It.IsAny<CancellationToken>()),
                Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync("passenger:all", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NotFound_ReturnsFalse()
        {
            var result = await _service.DeleteAsync("NOPE");
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_Existing_ReturnsTrue_AndClearsCache()
        {
            var db = TestDbContextFactory.CreateInMemory();
            var service = new PassengersService(db, Mock.Of<ILogger<PassengersService>>(), _cacheMock.Object);
            var existing = db.Passengers.First();

            var result = await service.DeleteAsync(existing.PassengerId);

            Assert.True(result);
            Assert.Null(db.Passengers.Find(existing.PassengerId));
            _cacheMock.Verify(c => c.RemoveAsync($"passenger:{existing.PassengerId}", It.IsAny<CancellationToken>()),
                Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync("passenger:all", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}