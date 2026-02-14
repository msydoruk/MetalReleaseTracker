using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Migrations.ParserService
{
    /// <inheritdoc />
    public partial class InitialParserService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "AlbumParsedEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParsingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventPayload = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumParsedEvents");

            migrationBuilder.DropTable(
                name: "ParsingSessions");
        }
    }
}
