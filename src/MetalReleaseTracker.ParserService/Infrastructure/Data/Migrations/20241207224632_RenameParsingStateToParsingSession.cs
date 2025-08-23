using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Migrations
{
    /// <inheritdoc />
    public partial class RenameParsingStateToParsingSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumParsedEvents_ParsingStates_ParsingStateId",
                table: "AlbumParsedEvents");

            migrationBuilder.DropTable(
                name: "ParsingStates");

            migrationBuilder.RenameColumn(
                name: "ParsingStateId",
                table: "AlbumParsedEvents",
                newName: "ParsingSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_AlbumParsedEvents_ParsingStateId",
                table: "AlbumParsedEvents",
                newName: "IX_AlbumParsedEvents_ParsingSessionId");

            migrationBuilder.CreateTable(
                name: "ParsingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributorCode = table.Column<int>(type: "integer", nullable: false),
                    PageToProcess = table.Column<string>(type: "text", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ParsingStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsingSessions", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumParsedEvents_ParsingSessions_ParsingSessionId",
                table: "AlbumParsedEvents",
                column: "ParsingSessionId",
                principalTable: "ParsingSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumParsedEvents_ParsingSessions_ParsingSessionId",
                table: "AlbumParsedEvents");

            migrationBuilder.DropTable(
                name: "ParsingSessions");

            migrationBuilder.RenameColumn(
                name: "ParsingSessionId",
                table: "AlbumParsedEvents",
                newName: "ParsingStateId");

            migrationBuilder.RenameIndex(
                name: "IX_AlbumParsedEvents_ParsingSessionId",
                table: "AlbumParsedEvents",
                newName: "IX_AlbumParsedEvents_ParsingStateId");

            migrationBuilder.CreateTable(
                name: "ParsingStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributorCode = table.Column<int>(type: "integer", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextPageToProcess = table.Column<string>(type: "text", nullable: false),
                    ParsingStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsingStates", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumParsedEvents_ParsingStates_ParsingStateId",
                table: "AlbumParsedEvents",
                column: "ParsingStateId",
                principalTable: "ParsingStates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
