using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IgniteLifeApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class new12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "refresh_tokens",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "ReplacedByTokenHash",
                table: "refresh_tokens",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "BowenServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsMultiSession = table.Column<bool>(type: "boolean", nullable: false),
                    SessionCount = table.Column<int>(type: "integer", nullable: true),
                    IsGroupSession = table.Column<bool>(type: "boolean", nullable: false),
                    MaxGroupSize = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ImageAltText = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BowenServices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BowenServices_IsActive",
                table: "BowenServices",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BowenServices_IsGroupSession_IsMultiSession",
                table: "BowenServices",
                columns: new[] { "IsGroupSession", "IsMultiSession" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BowenServices");

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "refresh_tokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "ReplacedByTokenHash",
                table: "refresh_tokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
