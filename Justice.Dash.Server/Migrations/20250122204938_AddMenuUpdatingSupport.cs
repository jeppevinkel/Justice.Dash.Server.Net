using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Justice.Dash.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuUpdatingSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Dirty",
                table: "menu_items",
                newName: "NeedsVeganization");

            migrationBuilder.AddColumn<bool>(
                name: "NeedsDescription",
                table: "menu_items",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsFoodContents",
                table: "menu_items",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsImageRegeneration",
                table: "menu_items",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsNameCorrection",
                table: "menu_items",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsVeganDescription",
                table: "menu_items",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsVeganImageRegeneration",
                table: "menu_items",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsDescription",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "NeedsFoodContents",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "NeedsImageRegeneration",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "NeedsNameCorrection",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "NeedsVeganDescription",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "NeedsVeganImageRegeneration",
                table: "menu_items");

            migrationBuilder.RenameColumn(
                name: "NeedsVeganization",
                table: "menu_items",
                newName: "Dirty");
        }
    }
}
