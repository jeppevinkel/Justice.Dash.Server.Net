using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Justice.Dash.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodModifierTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FoodModifier",
                table: "menu_items");

            migrationBuilder.AddColumn<Guid>(
                name: "FoodModifierId",
                table: "menu_items",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "food_modifiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_modifiers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_FoodModifierId",
                table: "menu_items",
                column: "FoodModifierId");

            migrationBuilder.AddForeignKey(
                name: "FK_menu_items_food_modifiers_FoodModifierId",
                table: "menu_items",
                column: "FoodModifierId",
                principalTable: "food_modifiers",
                principalColumn: "Id");

            migrationBuilder.InsertData(
                table: "food_modifiers",
                columns: new[] {"Id", "Title", "Description"},
                values: new object[,]
                {
                    {Guid.NewGuid(), "Blue Food", "The food is heavily colored blue."},
                    {Guid.NewGuid(), "Red Food", "The food is heavily colored red."},
                    {Guid.NewGuid(), "Green Food", "The food is heavily colored green."},
                    {Guid.NewGuid(), "Yellow Food", "The food is heavily colored yellow."},
                    {Guid.NewGuid(), "Purple Food", "The food is heavily colored purple."},
                    {Guid.NewGuid(), "White Food", "The food is heavily colored white."},
                    {Guid.NewGuid(), "Black Food", "The food is heavily colored black."},
                    {Guid.NewGuid(), "Pixar Style", "The plate is presented in the style of Pixar."},
                    {Guid.NewGuid(), "DreamWorks Style", "The plate is presented in the style of Dream Works."},
                    {Guid.NewGuid(), "Disney Style", "The plate is presented in the style of Disney."},
                    {Guid.NewGuid(), "VHS Film", "The plate is presented in the style of an old analog VHS film."},
                    {
                        Guid.NewGuid(), "18th Century",
                        "The plate is presented in the style an image from the 18th century."
                    },
                    {Guid.NewGuid(), "Midnight Serving", "The plate is presented in the middle of the night."},
                    {Guid.NewGuid(), "Raw Food", "The food is raw."},
                    {Guid.NewGuid(), "Burnt Food", "The food is burnt."},
                    {Guid.NewGuid(), "American Cuisine", "The food is from america."},
                    {Guid.NewGuid(), "South American Cuisine", "The food is from south america."},
                    {Guid.NewGuid(), "African Cuisine", "The food is from africa."},
                    {Guid.NewGuid(), "Asian Cuisine", "The food is from asia."},
                    {Guid.NewGuid(), "Antarctic Cuisine", "The food is from antarctica."},
                    {Guid.NewGuid(), "Futuristic Food", "The food is from the far future."},
                    {Guid.NewGuid(), "Fantasy Food", "The food is from fantasy land."},
                    {Guid.NewGuid(), "Post-Apocalyptic", "The food is from post apocalyptic future."},
                    {Guid.NewGuid(), "Table Spread", "The food is spread across the table."},
                    {Guid.NewGuid(), "Forest Floor", "The food is served on a lush forest floor."},
                    {Guid.NewGuid(), "Deep Ocean", "The food is served deep down in the ocean."},
                    {Guid.NewGuid(), "Space Dining", "The food is served in space."},
                    {Guid.NewGuid(), "Weird Plating", "The plating is weird."},
                    {Guid.NewGuid(), "Traditional Plating", "The plating is very traditional."},
                    {Guid.NewGuid(), "Inverted Plating", "The plating is upside down."},
                    {Guid.NewGuid(), "Fine Dining", "The plating is like a fine dining restaurant."},
                    {Guid.NewGuid(), "Inn Style", "The plating is like a hearthy warm inn."}
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_menu_items_food_modifiers_FoodModifierId",
                table: "menu_items");

            migrationBuilder.DropTable(
                name: "food_modifiers");

            migrationBuilder.DropIndex(
                name: "IX_menu_items_FoodModifierId",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "FoodModifierId",
                table: "menu_items");

            migrationBuilder.AddColumn<string>(
                name: "FoodModifier",
                table: "menu_items",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
