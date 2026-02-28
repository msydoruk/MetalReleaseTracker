using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveParsedTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParsedTitle",
                table: "Albums");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParsedTitle",
                table: "Albums",
                type: "text",
                nullable: true);
        }
    }
}
