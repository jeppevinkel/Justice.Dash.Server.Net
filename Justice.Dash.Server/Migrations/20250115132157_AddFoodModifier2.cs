using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Justice.Dash.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodModifier2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FoodModifier",
                table: "menu_items",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FoodModifier",
                table: "menu_items");
        }
    }
}
