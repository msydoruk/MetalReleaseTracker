using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeoSettingsAndEntitySeoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "Bands",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoKeywords",
                table: "Bands",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "Bands",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "Albums",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoKeywords",
                table: "Albums",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "Albums",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO "Settings" ("Key", "Value", "Category", "UpdatedAt")
                VALUES
                    ('SiteName', 'Metal Release Tracker', 'Seo', NOW()),
                    ('SiteUrl', 'https://metal-release.com', 'Seo', NOW()),
                    ('DefaultOgImage', 'https://metal-release.com/logo512.png', 'Seo', NOW()),
                    ('DefaultMetaDescription', 'Track and buy physical releases (vinyl, CD, tape) of Ukrainian metal bands from foreign distributors and labels. One catalog, direct links to stores.', 'Seo', NOW()),
                    ('ContactEmail', 'metal.release.tracker@gmail.com', 'Seo', NOW()),
                    ('OrganizationName', 'Metal Release Tracker', 'Seo', NOW()),
                    ('OrganizationLogoUrl', 'https://metal-release.com/logo512.png', 'Seo', NOW()),
                    ('RobotsTxt', E'User-agent: *\nDisallow: /login\nDisallow: /register\nDisallow: /profile\nDisallow: /auth/\nDisallow: /signin-callback', 'Seo', NOW()),
                    ('Model', 'claude-sonnet-4-20250514', 'AiSeo', NOW()),
                    ('MaxTokens', '1024', 'AiSeo', NOW()),
                    ('IsEnabled', 'true', 'AiSeo', NOW())
                ON CONFLICT ("Key") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeoDescription",
                table: "Bands");

            migrationBuilder.DropColumn(
                name: "SeoKeywords",
                table: "Bands");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                table: "Bands");

            migrationBuilder.DropColumn(
                name: "SeoDescription",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "SeoKeywords",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                table: "Albums");
        }
    }
}
