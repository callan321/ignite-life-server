using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IgniteLifeApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class refhreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_AdminUsers_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "refresh_tokens");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UserId",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_ExpiresAtUtc",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_ExpiresAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "refresh_tokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "refresh_tokens",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "refresh_tokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevokedAtUtc",
                table: "refresh_tokens",
                type: "timestamptz",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReplacedByTokenHash",
                table: "refresh_tokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "refresh_tokens",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAtUtc",
                table: "refresh_tokens",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "refresh_tokens",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_TokenHash_RevokedAtUtc",
                table: "refresh_tokens",
                columns: new[] { "TokenHash", "RevokedAtUtc" },
                filter: "\"RevokedAtUtc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId_ExpiresAtUtc",
                table: "refresh_tokens",
                columns: new[] { "UserId", "ExpiresAtUtc" },
                filter: "\"RevokedAtUtc\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_AdminUsers_UserId",
                table: "refresh_tokens",
                column: "UserId",
                principalTable: "AdminUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_AdminUsers_UserId",
                table: "refresh_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_TokenHash_RevokedAtUtc",
                table: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_UserId_ExpiresAtUtc",
                table: "refresh_tokens");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                newName: "RefreshTokens");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_UserId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_TokenHash",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_ExpiresAtUtc",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_ExpiresAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "RefreshTokens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "RefreshTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevokedAtUtc",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReplacedByTokenHash",
                table: "RefreshTokens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "RefreshTokens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(45)",
                oldMaxLength: 45,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAtUtc",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_AdminUsers_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "AdminUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
