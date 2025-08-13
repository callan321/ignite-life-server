using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IgniteLifeApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class utx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpenTime",
                table: "booking_rule_opening_hours",
                newName: "OpenTimeUtc");

            migrationBuilder.RenameColumn(
                name: "CloseTime",
                table: "booking_rule_opening_hours",
                newName: "CloseTimeUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpenTimeUtc",
                table: "booking_rule_opening_hours",
                newName: "OpenTime");

            migrationBuilder.RenameColumn(
                name: "CloseTimeUtc",
                table: "booking_rule_opening_hours",
                newName: "CloseTime");
        }
    }
}
