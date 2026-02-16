using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Migrations.ParserService
{
    /// <inheritdoc />
    public partial class RemoveAliasesFromBandReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aliases",
                table: "BandReferences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "Aliases",
                table: "BandReferences",
                type: "text[]",
                nullable: false);
        }
    }
}
