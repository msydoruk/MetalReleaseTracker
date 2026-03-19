using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameFeedbacksToReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Feedbacks",
                newName: "Reviews");

            migrationBuilder.RenameIndex(
                name: "PK_Feedbacks",
                table: "Reviews",
                newName: "PK_Reviews");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Reviews");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Reviews",
                type: "text",
                nullable: false,
                defaultValue: "Anonymous");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Reviews");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Reviews",
                type: "text",
                nullable: true);

            migrationBuilder.RenameIndex(
                name: "PK_Reviews",
                table: "Reviews",
                newName: "PK_Feedbacks");

            migrationBuilder.RenameTable(
                name: "Reviews",
                newName: "Feedbacks");
        }
    }
}
