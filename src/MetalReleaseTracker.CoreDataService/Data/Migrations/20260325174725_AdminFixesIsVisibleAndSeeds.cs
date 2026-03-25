using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdminFixesIsVisibleAndSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Distributors",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Bands",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql("""
                UPDATE "Distributors" SET "IsVisible" = true;
                UPDATE "Bands" SET "IsVisible" = true;
                """);

            migrationBuilder.Sql("""
                INSERT INTO "Settings" ("Key", "Value", "Category", "UpdatedAt")
                VALUES
                    ('GoogleSiteVerification', 'cwZlE3hwgiJ3ICn3rPVZrddw_eX8Nuo4PdUeoQmrIhA', 'Seo', NOW()),
                    ('BandPrompt', E'You are an SEO expert specializing in heavy metal music websites.\nGenerate optimized SEO metadata for the following band page.\n\nBand Name: {{bandName}}\nGenre: {{genre}}\nCountry: Ukraine\nFormation Year: {{formationYear}}\nDescription: {{description}}\nAlbum Count: {{albumCount}}\n\nRULES:\n1. Title must be under 60 characters, include band name and key genre.\n2. Description must be under 155 characters, compelling for search results, include band name, genre, and \"Ukrainian\".\n3. Keywords: 5-10 comma-separated terms relevant for this band''s page (include band name, genre, \"Ukrainian metal\", album-related terms).\n4. Write in English.\n5. Do NOT use generic filler. Be specific to this band.\n\nRespond ONLY with JSON:\n{\"seoTitle\": \"string\", \"seoDescription\": \"string\", \"seoKeywords\": \"keyword1, keyword2, keyword3\"}', 'AiSeo', NOW()),
                    ('AlbumPrompt', E'You are an SEO expert specializing in heavy metal music e-commerce.\nGenerate optimized SEO metadata for the following album product page.\n\nBand Name: {{bandName}}\nAlbum Title: {{albumTitle}}\nGenre: {{genre}}\nYear: {{year}}\nMedia Format: {{media}}\nPrice: EUR {{price}}\nLabel: {{label}}\nDescription: {{description}}\n\nRULES:\n1. Title must be under 60 characters. Format: \"Album by Band - Format | Buy Online\" or similar.\n2. Description must be under 155 characters, compelling for search results. Include band, album, format, and call to action.\n3. Keywords: 5-10 comma-separated terms (include band name, album name, genre, format, \"buy\", \"vinyl\"/\"CD\" as relevant).\n4. Write in English.\n5. Focus on purchase intent - this is a product page.\n\nRespond ONLY with JSON:\n{\"seoTitle\": \"string\", \"seoDescription\": \"string\", \"seoKeywords\": \"keyword1, keyword2, keyword3\"}', 'AiSeo', NOW())
                ON CONFLICT ("Key") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Distributors");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Bands");
        }
    }
}
