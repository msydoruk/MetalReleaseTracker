using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Migrations.ParserService
{
    /// <inheritdoc />
    public partial class AddBandReferencesAndCatalogueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BandReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BandName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Aliases = table.Column<List<string>>(type: "text[]", nullable: false),
                    MetalArchivesId = table.Column<long>(type: "bigint", nullable: false),
                    Genre = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BandReferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogueIndex",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributorCode = table.Column<int>(type: "integer", nullable: false),
                    BandName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AlbumTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RawTitle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DetailUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogueIndex", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BandReferences_BandName",
                table: "BandReferences",
                column: "BandName");

            migrationBuilder.CreateIndex(
                name: "IX_BandReferences_MetalArchivesId",
                table: "BandReferences",
                column: "MetalArchivesId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogueIndex_DetailUrl_DistributorCode",
                table: "CatalogueIndex",
                columns: new[] { "DetailUrl", "DistributorCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogueIndex_DistributorCode_Status",
                table: "CatalogueIndex",
                columns: new[] { "DistributorCode", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BandReferences");

            migrationBuilder.DropTable(
                name: "CatalogueIndex");
        }
    }
}
