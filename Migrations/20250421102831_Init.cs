using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    BufferBetweenBookingsMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookingServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Duration = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Auth0Id = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpeningExceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BookingRuleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpeningExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpeningExceptions_BookingRules_BookingRuleId",
                        column: x => x.BookingRuleId,
                        principalTable: "BookingRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpeningHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    OpenTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CloseTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    BookingRulesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpeningHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpeningHours_BookingRules_BookingRulesId",
                        column: x => x.BookingRulesId,
                        principalTable: "BookingRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_BookingServices_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "BookingServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Bookings_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInfos_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_Bookings_UserProfileId",
                table: "Bookings",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningExceptions_BookingRuleId",
                table: "OpeningExceptions",
                column: "BookingRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningHours_BookingRulesId",
                table: "OpeningHours",
                column: "BookingRulesId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningHours_DayOfWeek_BookingRulesId",
                table: "OpeningHours",
                columns: new[] { "DayOfWeek", "BookingRulesId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserInfos_UserProfileId",
                table: "UserInfos",
                column: "UserProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "OpeningExceptions");

            migrationBuilder.DropTable(
                name: "OpeningHours");

            migrationBuilder.DropTable(
                name: "UserInfos");

            migrationBuilder.DropTable(
                name: "BookingServices");

            migrationBuilder.DropTable(
                name: "BookingRules");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
