using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations.Identity
{
    /// <inheritdoc />
    public partial class AddCreatedDateToAspNetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "AspNetUsers"
                ADD COLUMN IF NOT EXISTS "CreatedDate" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "AspNetUsers"
                DROP COLUMN IF EXISTS "CreatedDate";
                """);
        }
    }
}
