using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PassengerService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AircompanyCode",
                table: "Flights",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ArrivalPortCode",
                table: "Flights",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeparturePortCode",
                table: "Flights",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FlightStatus",
                table: "Flights",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "LuggageWeight",
                table: "Bookings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "PaidCheckin",
                table: "Bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AircompanyCode",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrivalPortCode",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DeparturePortCode",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "FlightStatus",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "LuggageWeight",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaidCheckin",
                table: "Bookings");
        }
    }
}
