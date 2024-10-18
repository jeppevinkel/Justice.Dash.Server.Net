using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Justice.Dash.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddVeganizedContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VeganizedDescription",
                table: "menu_items",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VeganizedFoodName",
                table: "menu_items",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "VeganizedImageId",
                table: "menu_items",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_VeganizedImageId",
                table: "menu_items",
                column: "VeganizedImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_menu_items_images_VeganizedImageId",
                table: "menu_items",
                column: "VeganizedImageId",
                principalTable: "images",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_menu_items_images_VeganizedImageId",
                table: "menu_items");

            migrationBuilder.DropIndex(
                name: "IX_menu_items_VeganizedImageId",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "VeganizedDescription",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "VeganizedFoodName",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "VeganizedImageId",
                table: "menu_items");
        }
    }
}
