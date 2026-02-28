using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Migrations.ParserService
{
    /// <inheritdoc />
    public partial class AddSettingsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiAgents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SystemPrompt = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MaxTokens = table.Column<int>(type: "integer", nullable: false),
                    MaxConcurrentRequests = table.Column<int>(type: "integer", nullable: false),
                    DelayBetweenBatchesMs = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAgents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParsingSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributorCode = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParsingUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsingSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Key);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiAgents_IsActive",
                table: "AiAgents",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ParsingSources_DistributorCode",
                table: "ParsingSources",
                column: "DistributorCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Category",
                table: "Settings",
                column: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiAgents");

            migrationBuilder.DropTable(
                name: "ParsingSources");

            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
