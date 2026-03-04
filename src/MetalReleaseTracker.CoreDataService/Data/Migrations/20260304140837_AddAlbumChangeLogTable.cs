using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAlbumChangeLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlbumChangeLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlbumName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BandName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DistributorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Price = table.Column<float>(type: "real", nullable: false),
                    PurchaseUrl = table.Column<string>(type: "text", nullable: true),
                    ChangeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumChangeLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumChangeLogs_ChangedAt",
                table: "AlbumChangeLogs",
                column: "ChangedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumChangeLogs");
        }
    }
}
