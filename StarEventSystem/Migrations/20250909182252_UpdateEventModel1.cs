using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarEventSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEventModel1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizerName",
                table: "Event");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Event",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizerId",
                table: "Event",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "OrganizerId",
                table: "Event");

            migrationBuilder.AddColumn<string>(
                name: "OrganizerName",
                table: "Event",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
