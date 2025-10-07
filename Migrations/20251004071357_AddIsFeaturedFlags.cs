using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthyolBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFeaturedFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Hospitals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "HealthServices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Doctors",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Hospitals");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "HealthServices");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Doctors");
        }
    }
}
