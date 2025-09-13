using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarEventSystem.Migrations
{
    /// <inheritdoc />
    public partial class NewtableAd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QrCodeBase64",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QrCodeBase64",
                table: "Orders");
        }
    }
}
