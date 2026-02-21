using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Migrations.ParserService
{
    /// <inheritdoc />
    public partial class AddAiVerificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogueIndexId = table.Column<Guid>(type: "uuid", nullable: false),
                    BandName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AlbumTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsUkrainian = table.Column<bool>(type: "boolean", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "double precision", nullable: false),
                    AiAnalysis = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    AdminDecision = table.Column<int>(type: "integer", nullable: true),
                    AdminDecisionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiVerifications_CatalogueIndex_CatalogueIndexId",
                        column: x => x.CatalogueIndexId,
                        principalTable: "CatalogueIndex",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiVerifications_CatalogueIndexId_CreatedAt",
                table: "AiVerifications",
                columns: new[] { "CatalogueIndexId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiVerifications");
        }
    }
}
