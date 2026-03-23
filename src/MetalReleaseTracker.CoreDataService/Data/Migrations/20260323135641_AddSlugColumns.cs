using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Bands",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Albums",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: string.Empty);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bands_Slug",
                table: "Bands");

            migrationBuilder.DropIndex(
                name: "IX_Albums_Slug",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Bands");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Albums");
        }
    }
}
