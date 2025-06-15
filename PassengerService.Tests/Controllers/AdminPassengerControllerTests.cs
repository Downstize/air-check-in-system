using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PassengerService.Controllers;
using PassengerService.Services;
using Shared.Contracts;

namespace PassengerService.Tests.Controllers
{
    public class AdminPassengerControllerTests
    {
        private readonly AdminPassengerController _controller;
        private readonly Mock<IPassengersService> _serviceMock;

        public AdminPassengerControllerTests()
        {
            _serviceMock = new Mock<IPassengersService>();
            var loggerMock = new Mock<ILogger<AdminPassengerController>>();
            _controller = new AdminPassengerController(_serviceMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithDtos()
        {
            var dto = new PassengerDto
            {
                PassengerId = "A1",
                PnrId = "PNR1",
                PaxNo = 1,
                LastName = "Test",
                FirstName = "User",
                BirthDate = DateTime.UtcNow,
                Category = "ADT",
                CheckInStatus = "none",
                SeatsOccupied = 1,
                Eticket = true,
                Remarks = new List<string>(),
                Apis = 0,
                BookingId = 1,
                Document = new PassengerDocumentDto
                {
                    Type = "passport",
                    IssueCountryCode = "RU",
                    Number = "P0001",
                    NationalityCode = "RU",
                    BirthDate = DateTime.UtcNow.AddYears(-30),
                    ExpiryDate = DateTime.UtcNow.AddYears(5)
                },
                VisaDocument = new PassengerVisaDocumentDto
                {
                    BirthPlace = "Moscow",
                    Number = "V0001",
                    IssuePlace = "Moscow Consulate",
                    IssueDate = DateTime.UtcNow.AddYears(-2),
                    ApplicCountryCode = "US"
                },
                SeatNumber = "1A",
                SeatStatus = "free",
                SeatLayerType = "economy"
            };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new[] { dto });

            var action = await _controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(action);
            var actual = Assert.IsType<List<PassengerDto>>(ok.Value);
            Assert.Single(actual);
            Assert.Equal(dto.PassengerId, actual[0].PassengerId);
            Assert.Equal(dto.PnrId, actual[0].PnrId);
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync("nope")).ReturnsAsync((PassengerDto?)null);
            var action = await _controller.GetById("nope");
            Assert.IsType<NotFoundResult>(action);
        }

        [Fact]
        public async Task Create_ReturnsCreated()
        {
            var dto = new PassengerDto
            {
                PassengerId = "Z1", PnrId = "PNR1", PaxNo = 1, LastName = "New", FirstName = "User",
                BirthDate = DateTime.UtcNow, Category = "ADT", CheckInStatus = "none", SeatsOccupied = 1,
                Eticket = true, Remarks = new List<string>(), Apis = 0, BookingId = 1,
                Document = new PassengerDocumentDto
                {
                    Type = "passport", IssueCountryCode = "RU", Number = "P0002", NationalityCode = "RU",
                    BirthDate = DateTime.UtcNow.AddYears(-30), ExpiryDate = DateTime.UtcNow.AddYears(5)
                },
                VisaDocument = new PassengerVisaDocumentDto
                {
                    BirthPlace = "Moscow", Number = "V0002", IssuePlace = "Moscow Consulate",
                    IssueDate = DateTime.UtcNow.AddYears(-1), ApplicCountryCode = "US"
                },
                SeatNumber = "1B", SeatStatus = "free", SeatLayerType = "economy"
            };
            _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(dto);

            var action = await _controller.Create(dto);
            var created = Assert.IsType<CreatedAtActionResult>(action);
            Assert.Equal(dto, Assert.IsType<PassengerDto>(created.Value));
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            var dto = new PassengerDto { PassengerId = "X1" };
            var action = await _controller.Update("Y1", dto);
            Assert.IsType<BadRequestObjectResult>(action);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.DeleteAsync("nope")).ReturnsAsync(false);
            var action = await _controller.Delete("nope");
            Assert.IsType<NotFoundResult>(action);
        }

        [Fact]
        public async Task Delete_Found_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.DeleteAsync("yes")).ReturnsAsync(true);
            var action = await _controller.Delete("yes");
            Assert.IsType<NoContentResult>(action);
        }
    }
}