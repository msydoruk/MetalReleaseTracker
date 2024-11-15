using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMediaTypeToNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Media",
                table: "Albums",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
               name: "Media",
               table: "Albums",
               type: "character varying(50)",
               maxLength: 50,
               nullable: false,
               defaultValue: " ",
               oldClrType: typeof(string),
               oldType: "character varying(50)",
               oldMaxLength: 50,
               oldNullable: true);
        }
    }
}
