using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Migrations.ParserService
{
    /// <inheritdoc />
    public partial class RemoveCatalogSyncConsolidateIntoParser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migrate CatalogueIndexStatus enum values:
            // Old: New=0, Relevant=1, NotRelevant=2, Processed=3, AiVerified=4
            // New: New=0, Relevant=1, NotRelevant=2, AiVerified=3, Deleted=4
            // Step 1: Move old AiVerified(4) to new AiVerified(3)
            // Step 2: Move old Processed(3) to new AiVerified(3) as well
            migrationBuilder.Sql("""UPDATE "CatalogueIndex" SET "Status" = 3 WHERE "Status" IN (3, 4)""");

            migrationBuilder.DropTable(
                name: "AlbumParsedEvents");

            migrationBuilder.DropTable(
                name: "ParsingSessions");

            migrationBuilder.CreateTable(
                name: "CatalogueIndexDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogueIndexId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeType = table.Column<int>(type: "integer", nullable: false),
                    PublicationStatus = table.Column<int>(type: "integer", nullable: false),
                    LastPublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DistributorCode = table.Column<int>(type: "integer", nullable: false),
                    BandName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SKU = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Genre = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Price = table.Column<float>(type: "real", nullable: false),
                    PurchaseUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PhotoUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Media = table.Column<int>(type: "integer", nullable: true),
                    Label = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Press = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: true),
                    CanonicalTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OriginalYear = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogueIndexDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogueIndexDetails_CatalogueIndex_CatalogueIndexId",
                        column: x => x.CatalogueIndexId,
                        principalTable: "CatalogueIndex",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogueIndexDetails_CatalogueIndexId",
                table: "CatalogueIndexDetails",
                column: "CatalogueIndexId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogueIndexDetails_ChangeType_PublicationStatus",
                table: "CatalogueIndexDetails",
                columns: new[] { "ChangeType", "PublicationStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogueIndexDetails");

            migrationBuilder.CreateTable(
                name: "ParsingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributorCode = table.Column<int>(type: "integer", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ParsingStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsingSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlbumParsedEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParsingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventPayload = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumParsedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumParsedEvents_ParsingSessions_ParsingSessionId",
                        column: x => x.ParsingSessionId,
                        principalTable: "ParsingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumParsedEvents_ParsingSessionId",
                table: "AlbumParsedEvents",
                column: "ParsingSessionId");
        }
    }
}
