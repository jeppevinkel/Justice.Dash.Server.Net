using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Justice.Dash.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "weather",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsRaining = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RainAmount = table.Column<double>(type: "double", nullable: true),
                    Temperature = table.Column<double>(type: "double", nullable: true),
                    Humidity = table.Column<double>(type: "double", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weather", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "weather");
        }
    }
}
