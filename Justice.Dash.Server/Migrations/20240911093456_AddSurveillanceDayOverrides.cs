using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Justice.Dash.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveillanceDayOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "surveillance_day_overrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Responsible = table.Column<string>(type: "text", nullable: false),
                    WeeklySurveillanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_surveillance_day_overrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_surveillance_day_overrides_surveillance_WeeklySurveillanceId",
                        column: x => x.WeeklySurveillanceId,
                        principalTable: "surveillance",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_surveillance_day_overrides_Type_Date",
                table: "surveillance_day_overrides",
                columns: new[] { "Type", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_surveillance_day_overrides_WeeklySurveillanceId",
                table: "surveillance_day_overrides",
                column: "WeeklySurveillanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "surveillance_day_overrides");
        }
    }
}