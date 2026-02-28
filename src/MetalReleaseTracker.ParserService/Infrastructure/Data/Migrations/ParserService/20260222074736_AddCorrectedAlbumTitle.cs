using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Migrations.ParserService
{
    /// <inheritdoc />
    public partial class AddCorrectedAlbumTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrectedAlbumTitle",
                table: "CatalogueIndex",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrectedAlbumTitle",
                table: "AiVerifications",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectedAlbumTitle",
                table: "CatalogueIndex");

            migrationBuilder.DropColumn(
                name: "CorrectedAlbumTitle",
                table: "AiVerifications");
        }
    }
}
