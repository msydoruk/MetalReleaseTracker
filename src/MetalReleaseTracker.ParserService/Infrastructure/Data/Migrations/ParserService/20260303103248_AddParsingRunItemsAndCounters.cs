using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Migrations.ParserService
{
    /// <inheritdoc />
    public partial class AddParsingRunItemsAndCounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountersJson",
                table: "ParsingRuns",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParsingRunItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParsingRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Categories = table.Column<string[]>(type: "text[]", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsingRunItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParsingRunItems_ParsingRuns_ParsingRunId",
                        column: x => x.ParsingRunId,
                        principalTable: "ParsingRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParsingRunItems_ParsingRunId",
                table: "ParsingRunItems",
                column: "ParsingRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParsingRunItems");

            migrationBuilder.DropColumn(
                name: "CountersJson",
                table: "ParsingRuns");
        }
    }
}
