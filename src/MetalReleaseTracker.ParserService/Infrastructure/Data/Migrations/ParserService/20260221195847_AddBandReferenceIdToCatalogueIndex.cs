using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Migrations.ParserService
{
    /// <inheritdoc />
    public partial class AddBandReferenceIdToCatalogueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BandReferenceId",
                table: "CatalogueIndex",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogueIndex_BandReferenceId",
                table: "CatalogueIndex",
                column: "BandReferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogueIndex_BandReferences_BandReferenceId",
                table: "CatalogueIndex",
                column: "BandReferenceId",
                principalTable: "BandReferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql("""
                UPDATE "CatalogueIndex" ci
                SET "BandReferenceId" = br."Id"
                FROM "BandReferences" br
                WHERE LOWER(ci."BandName") = LOWER(br."BandName")
                  AND ci."BandReferenceId" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogueIndex_BandReferences_BandReferenceId",
                table: "CatalogueIndex");

            migrationBuilder.DropIndex(
                name: "IX_CatalogueIndex_BandReferenceId",
                table: "CatalogueIndex");

            migrationBuilder.DropColumn(
                name: "BandReferenceId",
                table: "CatalogueIndex");
        }
    }
}
