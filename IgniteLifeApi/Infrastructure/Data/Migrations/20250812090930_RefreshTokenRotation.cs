using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IgniteLifeApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefreshTokenRotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Token",
                table: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "ExpiryDate",
                table: "RefreshToken",
                newName: "ExpiresAtUtc");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "RefreshToken",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "RefreshToken",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReplacedByTokenHash",
                table: "RefreshToken",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAtUtc",
                table: "RefreshToken",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "RefreshToken",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "RefreshToken",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_ExpiresAtUtc",
                table: "RefreshToken",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_TokenHash",
                table: "RefreshToken",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshToken_ExpiresAtUtc",
                table: "RefreshToken");

            migrationBuilder.DropIndex(
                name: "IX_RefreshToken_TokenHash",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "ReplacedByTokenHash",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "RevokedAtUtc",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "ExpiresAtUtc",
                table: "RefreshToken",
                newName: "ExpiryDate");

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "RefreshToken",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
