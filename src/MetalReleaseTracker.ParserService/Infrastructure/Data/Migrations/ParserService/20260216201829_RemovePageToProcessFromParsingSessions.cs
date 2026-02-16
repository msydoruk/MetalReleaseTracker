using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Migrations.ParserService
{
    /// <inheritdoc />
    public partial class RemovePageToProcessFromParsingSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageToProcess",
                table: "ParsingSessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PageToProcess",
                table: "ParsingSessions",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
        }
    }
}
