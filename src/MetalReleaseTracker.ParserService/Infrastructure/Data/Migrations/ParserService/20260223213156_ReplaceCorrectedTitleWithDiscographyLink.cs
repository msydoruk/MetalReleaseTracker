using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Migrations.ParserService
{
    /// <inheritdoc />
    public partial class ReplaceCorrectedTitleWithDiscographyLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectedAlbumTitle",
                table: "CatalogueIndex");

            migrationBuilder.DropColumn(
                name: "CorrectedAlbumTitle",
                table: "AiVerifications");

            migrationBuilder.AddColumn<Guid>(
                name: "BandDiscographyId",
                table: "CatalogueIndex",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MatchedBandDiscographyId",
                table: "AiVerifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogueIndex_BandDiscographyId",
                table: "CatalogueIndex",
                column: "BandDiscographyId");

            migrationBuilder.CreateIndex(
                name: "IX_AiVerifications_MatchedBandDiscographyId",
                table: "AiVerifications",
                column: "MatchedBandDiscographyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiVerifications_BandDiscography_MatchedBandDiscographyId",
                table: "AiVerifications",
                column: "MatchedBandDiscographyId",
                principalTable: "BandDiscography",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogueIndex_BandDiscography_BandDiscographyId",
                table: "CatalogueIndex",
                column: "BandDiscographyId",
                principalTable: "BandDiscography",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiVerifications_BandDiscography_MatchedBandDiscographyId",
                table: "AiVerifications");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogueIndex_BandDiscography_BandDiscographyId",
                table: "CatalogueIndex");

            migrationBuilder.DropIndex(
                name: "IX_CatalogueIndex_BandDiscographyId",
                table: "CatalogueIndex");

            migrationBuilder.DropIndex(
                name: "IX_AiVerifications_MatchedBandDiscographyId",
                table: "AiVerifications");

            migrationBuilder.DropColumn(
                name: "BandDiscographyId",
                table: "CatalogueIndex");

            migrationBuilder.DropColumn(
                name: "MatchedBandDiscographyId",
                table: "AiVerifications");

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
    }
}
