using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.ParserService.Migrations
{
    /// <inheritdoc />
    public partial class ParsingStateChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "AlbumParsedEvents");

            migrationBuilder.DropColumn(
                name: "PublishedDate",
                table: "AlbumParsedEvents");

            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                table: "ParsingStates",
                newName: "LastUpdatedDate");

            migrationBuilder.AddColumn<int>(
                name: "ParsingStatus",
                table: "ParsingStates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ParsingStateId",
                table: "AlbumParsedEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AlbumParsedEvents_ParsingStateId",
                table: "AlbumParsedEvents",
                column: "ParsingStateId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumParsedEvents_ParsingStates_ParsingStateId",
                table: "AlbumParsedEvents",
                column: "ParsingStateId",
                principalTable: "ParsingStates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumParsedEvents_ParsingStates_ParsingStateId",
                table: "AlbumParsedEvents");

            migrationBuilder.DropIndex(
                name: "IX_AlbumParsedEvents_ParsingStateId",
                table: "AlbumParsedEvents");

            migrationBuilder.DropColumn(
                name: "ParsingStatus",
                table: "ParsingStates");

            migrationBuilder.DropColumn(
                name: "ParsingStateId",
                table: "AlbumParsedEvents");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedDate",
                table: "ParsingStates",
                newName: "LastUpdated");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "AlbumParsedEvents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedDate",
                table: "AlbumParsedEvents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
