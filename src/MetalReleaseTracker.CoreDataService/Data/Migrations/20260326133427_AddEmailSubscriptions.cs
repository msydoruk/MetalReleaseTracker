using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    VerificationTokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnsubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailSubscriptions_Email",
                table: "EmailSubscriptions",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSubscriptions_UserId",
                table: "EmailSubscriptions",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailSubscriptions_VerificationToken",
                table: "EmailSubscriptions",
                column: "VerificationToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailSubscriptions");
        }
    }
}
