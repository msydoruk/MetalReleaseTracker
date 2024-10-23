using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOsmoseProductionsDistributor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Distributors",
                columns: new[] { "Id", "Name", "ParsingUrl", "Code" },
                values: new object[] { Guid.NewGuid(), "Osmose Productions", "https://www.osmoseproductions.com/liste/index.cfm?what=all&lng=2&tete=ukraine", 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Distributors",
                keyColumn: "Name",
                keyValue: "Osmose");
        }
    }
}
