using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCanonicalAlbumFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CanonicalTitle",
                table: "Albums",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalYear",
                table: "Albums",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParsedTitle",
                table: "Albums",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanonicalTitle",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "OriginalYear",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ParsedTitle",
                table: "Albums");
        }
    }
}
