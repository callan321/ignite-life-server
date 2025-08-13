using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IgniteLifeApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class zzz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingRuleOpeningHours_BookingRules_BookingRulesId",
                table: "BookingRuleOpeningHours");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "BookingServiceTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingRules",
                table: "BookingRules");

            migrationBuilder.DropIndex(
                name: "IX_BookingRules_IsDefault",
                table: "BookingRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingRuleOpeningHours",
                table: "BookingRuleOpeningHours");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingRuleBlockedPeriods",
                table: "BookingRuleBlockedPeriods");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "BookingRules");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "BookingRuleBlockedPeriods");

            migrationBuilder.DropColumn(
                name: "EndDateTime",
                table: "BookingRuleBlockedPeriods");

            migrationBuilder.DropColumn(
                name: "StartDateTime",
                table: "BookingRuleBlockedPeriods");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BookingRuleBlockedPeriods");

            migrationBuilder.RenameTable(
                name: "BookingRules",
                newName: "booking_rules");

            migrationBuilder.RenameTable(
                name: "BookingRuleOpeningHours",
                newName: "booking_rule_opening_hours");

            migrationBuilder.RenameTable(
                name: "BookingRuleBlockedPeriods",
                newName: "booking_rule_blocked_periods");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "refresh_tokens",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "AdminUsers",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AdminUsers",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_BookingRuleOpeningHours_DayOfWeek_BookingRulesId",
                table: "booking_rule_opening_hours",
                newName: "IX_booking_rule_opening_hours_DayOfWeek_BookingRulesId");

            migrationBuilder.RenameIndex(
                name: "IX_BookingRuleOpeningHours_BookingRulesId",
                table: "booking_rule_opening_hours",
                newName: "IX_booking_rule_opening_hours_BookingRulesId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "refresh_tokens",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "booking_rules",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SingletonKey",
                table: "booking_rules",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "booking_rules",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "booking_rules",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "booking_rule_opening_hours",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "booking_rule_opening_hours",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "booking_rule_opening_hours",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "booking_rule_blocked_periods",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BookingRulesId",
                table: "booking_rule_blocked_periods",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "booking_rule_blocked_periods",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDateTimeUtc",
                table: "booking_rule_blocked_periods",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateTimeUtc",
                table: "booking_rule_blocked_periods",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "booking_rule_blocked_periods",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_booking_rules",
                table: "booking_rules",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_booking_rule_opening_hours",
                table: "booking_rule_opening_hours",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_booking_rule_blocked_periods",
                table: "booking_rule_blocked_periods",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_booking_rules_Id",
                table: "booking_rules",
                column: "Id",
                unique: true,
                filter: "\"Id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_booking_rules_SingletonKey",
                table: "booking_rules",
                column: "SingletonKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_rule_blocked_periods_BookingRulesId",
                table: "booking_rule_blocked_periods",
                column: "BookingRulesId");

            migrationBuilder.AddForeignKey(
                name: "FK_booking_rule_blocked_periods_booking_rules_BookingRulesId",
                table: "booking_rule_blocked_periods",
                column: "BookingRulesId",
                principalTable: "booking_rules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_booking_rule_opening_hours_booking_rules_BookingRulesId",
                table: "booking_rule_opening_hours",
                column: "BookingRulesId",
                principalTable: "booking_rules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_booking_rule_blocked_periods_booking_rules_BookingRulesId",
                table: "booking_rule_blocked_periods");

            migrationBuilder.DropForeignKey(
                name: "FK_booking_rule_opening_hours_booking_rules_BookingRulesId",
                table: "booking_rule_opening_hours");

            migrationBuilder.DropPrimaryKey(
                name: "PK_booking_rules",
                table: "booking_rules");

            migrationBuilder.DropIndex(
                name: "IX_booking_rules_Id",
                table: "booking_rules");

            migrationBuilder.DropIndex(
                name: "IX_booking_rules_SingletonKey",
                table: "booking_rules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_booking_rule_opening_hours",
                table: "booking_rule_opening_hours");

            migrationBuilder.DropPrimaryKey(
                name: "PK_booking_rule_blocked_periods",
                table: "booking_rule_blocked_periods");

            migrationBuilder.DropIndex(
                name: "IX_booking_rule_blocked_periods_BookingRulesId",
                table: "booking_rule_blocked_periods");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "booking_rules");

            migrationBuilder.DropColumn(
                name: "SingletonKey",
                table: "booking_rules");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "booking_rules");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "booking_rules");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "booking_rule_opening_hours");

            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "booking_rule_opening_hours");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "booking_rule_opening_hours");

            migrationBuilder.DropColumn(
                name: "BookingRulesId",
                table: "booking_rule_blocked_periods");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "booking_rule_blocked_periods");

            migrationBuilder.DropColumn(
                name: "EndDateTimeUtc",
                table: "booking_rule_blocked_periods");

            migrationBuilder.DropColumn(
                name: "StartDateTimeUtc",
                table: "booking_rule_blocked_periods");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "booking_rule_blocked_periods");

            migrationBuilder.RenameTable(
                name: "booking_rules",
                newName: "BookingRules");

            migrationBuilder.RenameTable(
                name: "booking_rule_opening_hours",
                newName: "BookingRuleOpeningHours");

            migrationBuilder.RenameTable(
                name: "booking_rule_blocked_periods",
                newName: "BookingRuleBlockedPeriods");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "refresh_tokens",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "AdminUsers",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "AdminUsers",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_booking_rule_opening_hours_DayOfWeek_BookingRulesId",
                table: "BookingRuleOpeningHours",
                newName: "IX_BookingRuleOpeningHours_DayOfWeek_BookingRulesId");

            migrationBuilder.RenameIndex(
                name: "IX_booking_rule_opening_hours_BookingRulesId",
                table: "BookingRuleOpeningHours",
                newName: "IX_BookingRuleOpeningHours_BookingRulesId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "refresh_tokens",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "refresh_tokens",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "refresh_tokens",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "refresh_tokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "BookingRules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BookingRuleBlockedPeriods",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "BookingRuleBlockedPeriods",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDateTime",
                table: "BookingRuleBlockedPeriods",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateTime",
                table: "BookingRuleBlockedPeriods",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BookingRuleBlockedPeriods",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingRules",
                table: "BookingRules",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingRuleOpeningHours",
                table: "BookingRuleOpeningHours",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingRuleBlockedPeriods",
                table: "BookingRuleBlockedPeriods",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BookingServiceTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingServiceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_BookingServiceTypes_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "BookingServiceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingRules_IsDefault",
                table: "BookingRules",
                column: "IsDefault",
                unique: true,
                filter: "\"IsDefault\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ServiceId",
                table: "Bookings",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingServiceTypes_Name",
                table: "BookingServiceTypes",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingRuleOpeningHours_BookingRules_BookingRulesId",
                table: "BookingRuleOpeningHours",
                column: "BookingRulesId",
                principalTable: "BookingRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
