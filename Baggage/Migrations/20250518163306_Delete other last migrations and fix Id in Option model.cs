using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Baggage.Migrations
{
    /// <inheritdoc />
    public partial class DeleteotherlastmigrationsandfixIdinOptionmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""PaidOptions"" ALTER COLUMN ""PaidOptionId"" DROP IDENTITY IF EXISTS;");
            migrationBuilder.Sql(@"ALTER TABLE ""BaggagePayment"" ALTER COLUMN ""PaymentId"" DROP IDENTITY IF EXISTS;");

            migrationBuilder.AlterColumn<string>(
                name: "PaidOptionId",
                table: "PaidOptions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "BaggagePayment",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PaidOptionId",
                table: "PaidOptions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentId",
                table: "BaggagePayment",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
