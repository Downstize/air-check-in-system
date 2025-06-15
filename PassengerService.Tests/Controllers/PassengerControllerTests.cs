using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PassengerService.Controllers;
using PassengerService.Data;
using PassengerService.Tests.Utilities;
using Shared.Contracts;

namespace PassengerService.Tests.Controllers
{
    public class PassengerControllerTests
    {
        private readonly PassengerController _controller;
        private readonly ApplicationDbContext _db;

        public PassengerControllerTests()
        {
            _db = TestDbContextFactory.CreateInMemory();
            var logger = Mock.Of<ILogger<PassengerController>>();
            _controller = new PassengerController(_db, logger);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithSeededPassenger()
        {
            var action = await _controller.GetAll();
            
            var okResult = Assert.IsType<OkObjectResult>(action);
            var list = Assert.IsAssignableFrom<List<PassengerDto>>(okResult.Value);
            Assert.Contains(list, p => p.PassengerId == "EXISTING");
        }

        [Fact]
        public async Task Get_NonExisting_ReturnsNotFound()
        {
            var action = await _controller.Get("UNKNOWN");

            Assert.IsType<NotFoundResult>(action);
        }

        [Fact]
        public async Task Create_InvalidBooking_ReturnsBadRequest()
        {
            var badDto = new PassengerDto
            {
                BookingId = 999,
                PnrId = "X",
                PaxNo = 1,
                LastName = "L",
                FirstName = "F",
                BirthDate = DateTime.UtcNow,
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
            };
            
            var action = await _controller.Create(badDto);

            Assert.IsType<BadRequestObjectResult>(action);
        }

        [Fact]
        public async Task Create_Valid_ReturnsCreatedAtAction()
        {
            var dto = new PassengerDto
            {
                PassengerId = null,
                BookingId = 1,
                PnrId = "AB1234",
                PaxNo = 2,
                LastName = "Petrova",
                FirstName = "Maria",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                Category = "ADT",
                CheckInStatus = "none",
                SeatsOccupied = 1,
                Eticket = true,
                Remarks = new List<string>(),
                Apis = 0,
                SeatNumber = "10B",
                SeatStatus = "available",
                SeatLayerType = "economy",
                Document = new PassengerDocumentDto
                {
                    Type = "passport",
                    IssueCountryCode = "RU",
                    Number = "P1234562",
                    NationalityCode = "RU",
                    BirthDate = DateTime.UtcNow.AddYears(-30),
                    ExpiryDate = DateTime.UtcNow.AddYears(5)
                },
                VisaDocument = new PassengerVisaDocumentDto
                {
                    BirthPlace = "Moscow",
                    Number = "VISA002",
                    IssuePlace = "Moscow Consulate",
                    IssueDate = DateTime.UtcNow.AddYears(-1),
                    ApplicCountryCode = "US"
                }
            };
            
            var action = await _controller.Create(dto);
            
            var createdResult = Assert.IsType<CreatedAtActionResult>(action);
            var ret = Assert.IsType<PassengerDto>(createdResult.Value);
            Assert.False(string.IsNullOrEmpty(ret.PassengerId));
            
            var entity = await _db.Passengers.FindAsync(ret.PassengerId);
            Assert.NotNull(entity);
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            var dto = new PassengerDto { PassengerId = "A1", BookingId = 1, Document = new(), VisaDocument = new() };
            
            var action = await _controller.Update("B1", dto);
            
            Assert.IsType<BadRequestObjectResult>(action);
        }

        [Fact]
        public async Task Update_NotFound_ReturnsNotFound()
        {
            var dto = new PassengerDto { PassengerId = "NOPE", BookingId = 1, Document = new(), VisaDocument = new() };
            
            var action = await _controller.Update("NOPE", dto);
            
            Assert.IsType<NotFoundResult>(action);
        }

        [Fact]
        public async Task Update_BadBooking_ReturnsBadRequest()
        {
            var existing = await _db.Passengers.FirstAsync();
            var dto = new PassengerDto
            {
                PassengerId = existing.PassengerId,
                BookingId = 999,
                Document = new PassengerDocumentDto(),
                VisaDocument = new PassengerVisaDocumentDto()
            };
            
            var action = await _controller.Update(existing.PassengerId, dto);
            
            Assert.IsType<BadRequestObjectResult>(action);
        }

        [Fact]
        public async Task Update_Valid_ReturnsNoContent_AndPersists()
        {
            var existing = await _db.Passengers.FirstAsync();
            var dto = new PassengerDto
            {
                PassengerId = existing.PassengerId,
                BookingId = existing.BookingId,
                LastName = "Changed",
                PnrId = existing.PnrId,
                PaxNo = existing.PaxNo,
                BirthDate = existing.BirthDate,
                Category = existing.Category,
                CheckInStatus = existing.CheckInStatus,
                SeatsOccupied = existing.SeatsOccupied,
                Eticket = existing.Eticket,
                Document = new PassengerDocumentDto
                {
                    Type = "ID",
                    IssueCountryCode = "RU",
                    Number = "X",
                    NationalityCode = "RU",
                    BirthDate = existing.Document.BirthDate
                },
                VisaDocument = new PassengerVisaDocumentDto
                {
                    BirthPlace = "X",
                    Number = "V",
                    IssuePlace = "Y",
                    IssueDate = existing.VisaDocument.IssueDate,
                    ApplicCountryCode = existing.VisaDocument.ApplicCountryCode
                }
            };
            
            var action = await _controller.Update(existing.PassengerId, dto);
            
            Assert.IsType<NoContentResult>(action);
            
            var reloaded = await _db.Passengers.FindAsync(existing.PassengerId);
            Assert.Equal("Changed", reloaded.LastName);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            var action = await _controller.Delete("NO_SUCH");
            
            Assert.IsType<NotFoundResult>(action);
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent_AndRemoves()
        {
            var existing = await _db.Passengers.FirstAsync();
            
            var action = await _controller.Delete(existing.PassengerId);
            
            Assert.IsType<NoContentResult>(action);
            var deleted = await _db.Passengers.FindAsync(existing.PassengerId);
            Assert.Null(deleted);
        }
    }
}
