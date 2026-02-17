using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Migrations.ParserService
{
    /// <inheritdoc />
    public partial class AddBandDiscography : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BandDiscography",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BandReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlbumTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NormalizedAlbumTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AlbumType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BandDiscography", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BandDiscography_BandReferences_BandReferenceId",
                        column: x => x.BandReferenceId,
                        principalTable: "BandReferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BandDiscography_BandReferenceId",
                table: "BandDiscography",
                column: "BandReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_BandDiscography_BandReferenceId_NormalizedAlbumTitle",
                table: "BandDiscography",
                columns: new[] { "BandReferenceId", "NormalizedAlbumTitle" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BandDiscography");
        }
    }
}
