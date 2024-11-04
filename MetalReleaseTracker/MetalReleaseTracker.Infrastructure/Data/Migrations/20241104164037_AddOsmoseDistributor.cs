using MetalReleaseTracker.Core.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOsmoseDistributor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Distributors",
                columns: new[] { "Id", "Name", "ParsingUrl", "Code" },
                values: new object[] { Guid.NewGuid(), "Osmose", "https://www.osmoseproductions.com/liste/index.cfm?what=all&lng=2&tete=ukraine", (int)DistributorCode.OsmoseProductions });
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
