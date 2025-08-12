using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IgniteLifeApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitEntityConfigurations2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_BookingServiceType_ServiceId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_AdminUsers_UserId",
                table: "RefreshToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshToken",
                table: "RefreshToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingServiceType",
                table: "BookingServiceType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingRuleBlockedPeriod",
                table: "BookingRuleBlockedPeriod");

            migrationBuilder.RenameTable(
                name: "RefreshToken",
                newName: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "BookingServiceType",
                newName: "BookingServiceTypes");

            migrationBuilder.RenameTable(
                name: "BookingRuleBlockedPeriod",
                newName: "BookingRuleBlockedPeriods");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshToken_UserId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshToken_TokenHash",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshToken_ExpiresAtUtc",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_ExpiresAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_BookingServiceType_Name",
                table: "BookingServiceTypes",
                newName: "IX_BookingServiceTypes_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingServiceTypes",
                table: "BookingServiceTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingRuleBlockedPeriods",
                table: "BookingRuleBlockedPeriods",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_BookingServiceTypes_ServiceId",
                table: "Bookings",
                column: "ServiceId",
                principalTable: "BookingServiceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_AdminUsers_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "AdminUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_BookingServiceTypes_ServiceId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_AdminUsers_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingServiceTypes",
                table: "BookingServiceTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingRuleBlockedPeriods",
                table: "BookingRuleBlockedPeriods");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "RefreshToken");

            migrationBuilder.RenameTable(
                name: "BookingServiceTypes",
                newName: "BookingServiceType");

            migrationBuilder.RenameTable(
                name: "BookingRuleBlockedPeriods",
                newName: "BookingRuleBlockedPeriod");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshToken",
                newName: "IX_RefreshToken_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshToken",
                newName: "IX_RefreshToken_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_ExpiresAtUtc",
                table: "RefreshToken",
                newName: "IX_RefreshToken_ExpiresAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_BookingServiceTypes_Name",
                table: "BookingServiceType",
                newName: "IX_BookingServiceType_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshToken",
                table: "RefreshToken",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingServiceType",
                table: "BookingServiceType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingRuleBlockedPeriod",
                table: "BookingRuleBlockedPeriod",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_BookingServiceType_ServiceId",
                table: "Bookings",
                column: "ServiceId",
                principalTable: "BookingServiceType",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_AdminUsers_UserId",
                table: "RefreshToken",
                column: "UserId",
                principalTable: "AdminUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
