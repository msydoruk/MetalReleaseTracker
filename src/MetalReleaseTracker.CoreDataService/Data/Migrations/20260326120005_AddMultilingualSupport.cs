using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultilingualSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create Languages table and translation tables
            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NativeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "AlbumTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlbumId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    SeoTitle = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    SeoDescription = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    SeoKeywords = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumTranslations_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumTranslations_Languages_LanguageCode",
                        column: x => x.LanguageCode,
                        principalTable: "Languages",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BandTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BandId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SeoTitle = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    SeoDescription = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    SeoKeywords = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BandTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BandTranslations_Bands_BandId",
                        column: x => x.BandId,
                        principalTable: "Bands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BandTranslations_Languages_LanguageCode",
                        column: x => x.LanguageCode,
                        principalTable: "Languages",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DistributorTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributorId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributorTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistributorTranslations_Distributors_DistributorId",
                        column: x => x.DistributorId,
                        principalTable: "Distributors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DistributorTranslations_Languages_LanguageCode",
                        column: x => x.LanguageCode,
                        principalTable: "Languages",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NavigationItemTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NavigationItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SeoTitle = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    SeoDescription = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    SeoKeywords = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationItemTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NavigationItemTranslations_Languages_LanguageCode",
                        column: x => x.LanguageCode,
                        principalTable: "Languages",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NavigationItemTranslations_NavigationItems_NavigationItemId",
                        column: x => x.NavigationItemId,
                        principalTable: "NavigationItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsArticleTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NewsArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SeoTitle = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    SeoDescription = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    SeoKeywords = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticleTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsArticleTranslations_Languages_LanguageCode",
                        column: x => x.LanguageCode,
                        principalTable: "Languages",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NewsArticleTranslations_NewsArticles_NewsArticleId",
                        column: x => x.NewsArticleId,
                        principalTable: "NewsArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Code", "CreatedAt", "IsDefault", "IsEnabled", "Name", "NativeName", "SortOrder" },
                values: new object[,]
                {
                    { "en", new DateTime(2026, 3, 26, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "English", "English", 1 },
                    { "ua", new DateTime(2026, 3, 26, 0, 0, 0, 0, DateTimeKind.Utc), false, true, "Ukrainian", "Українська", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTranslations_AlbumId_LanguageCode",
                table: "AlbumTranslations",
                columns: new[] { "AlbumId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTranslations_LanguageCode",
                table: "AlbumTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_BandTranslations_BandId_LanguageCode",
                table: "BandTranslations",
                columns: new[] { "BandId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BandTranslations_LanguageCode",
                table: "BandTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_DistributorTranslations_DistributorId_LanguageCode",
                table: "DistributorTranslations",
                columns: new[] { "DistributorId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DistributorTranslations_LanguageCode",
                table: "DistributorTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_Languages_SortOrder",
                table: "Languages",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationItemTranslations_LanguageCode",
                table: "NavigationItemTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationItemTranslations_NavigationItemId_LanguageCode",
                table: "NavigationItemTranslations",
                columns: new[] { "NavigationItemId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticleTranslations_LanguageCode",
                table: "NewsArticleTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticleTranslations_NewsArticleId_LanguageCode",
                table: "NewsArticleTranslations",
                columns: new[] { "NewsArticleId", "LanguageCode" },
                unique: true);

            // Step 2: Add FK from Translations to Languages
            migrationBuilder.CreateIndex(
                name: "IX_Translations_Language",
                table: "Translations",
                column: "Language");

            migrationBuilder.AddForeignKey(
                name: "FK_Translations_Languages_Language",
                table: "Translations",
                column: "Language",
                principalTable: "Languages",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            // Step 3: Migrate existing data from old columns to translation tables
            migrationBuilder.Sql(@"
                INSERT INTO ""NewsArticleTranslations"" (""Id"", ""NewsArticleId"", ""LanguageCode"", ""Title"", ""Content"", ""SeoTitle"", ""SeoDescription"", ""SeoKeywords"")
                SELECT gen_random_uuid(), ""Id"", 'en', ""TitleEn"", ""ContentEn"", ""SeoTitle"", ""SeoDescription"", ""SeoKeywords""
                FROM ""NewsArticles""
                WHERE ""TitleEn"" IS NOT NULL AND ""TitleEn"" != '';

                INSERT INTO ""NewsArticleTranslations"" (""Id"", ""NewsArticleId"", ""LanguageCode"", ""Title"", ""Content"")
                SELECT gen_random_uuid(), ""Id"", 'ua', ""TitleUa"", ""ContentUa""
                FROM ""NewsArticles""
                WHERE ""TitleUa"" IS NOT NULL AND ""TitleUa"" != '';

                INSERT INTO ""NavigationItemTranslations"" (""Id"", ""NavigationItemId"", ""LanguageCode"", ""Title"", ""SeoTitle"", ""SeoDescription"", ""SeoKeywords"")
                SELECT gen_random_uuid(), ""Id"", 'en', ""TitleEn"", ""SeoTitle"", ""SeoDescription"", ""SeoKeywords""
                FROM ""NavigationItems""
                WHERE ""TitleEn"" IS NOT NULL AND ""TitleEn"" != '';

                INSERT INTO ""NavigationItemTranslations"" (""Id"", ""NavigationItemId"", ""LanguageCode"", ""Title"")
                SELECT gen_random_uuid(), ""Id"", 'ua', ""TitleUa""
                FROM ""NavigationItems""
                WHERE ""TitleUa"" IS NOT NULL AND ""TitleUa"" != '';

                INSERT INTO ""DistributorTranslations"" (""Id"", ""DistributorId"", ""LanguageCode"", ""Description"")
                SELECT gen_random_uuid(), ""Id"", 'en', ""DescriptionEn""
                FROM ""Distributors""
                WHERE ""DescriptionEn"" IS NOT NULL AND ""DescriptionEn"" != '';

                INSERT INTO ""DistributorTranslations"" (""Id"", ""DistributorId"", ""LanguageCode"", ""Description"")
                SELECT gen_random_uuid(), ""Id"", 'ua', ""DescriptionUa""
                FROM ""Distributors""
                WHERE ""DescriptionUa"" IS NOT NULL AND ""DescriptionUa"" != '';

                INSERT INTO ""BandTranslations"" (""Id"", ""BandId"", ""LanguageCode"", ""Description"", ""SeoTitle"", ""SeoDescription"", ""SeoKeywords"")
                SELECT gen_random_uuid(), ""Id"", 'en', ""Description"", ""SeoTitle"", ""SeoDescription"", ""SeoKeywords""
                FROM ""Bands""
                WHERE ""Description"" IS NOT NULL OR ""SeoTitle"" IS NOT NULL;

                INSERT INTO ""AlbumTranslations"" (""Id"", ""AlbumId"", ""LanguageCode"", ""SeoTitle"", ""SeoDescription"", ""SeoKeywords"")
                SELECT gen_random_uuid(), ""Id"", 'en', ""SeoTitle"", ""SeoDescription"", ""SeoKeywords""
                FROM ""Albums""
                WHERE ""SeoTitle"" IS NOT NULL;
            ");

            // Step 4: Drop old bilingual columns
            migrationBuilder.DropColumn(name: "ContentEn", table: "NewsArticles");
            migrationBuilder.DropColumn(name: "ContentUa", table: "NewsArticles");
            migrationBuilder.DropColumn(name: "SeoDescription", table: "NewsArticles");
            migrationBuilder.DropColumn(name: "SeoKeywords", table: "NewsArticles");
            migrationBuilder.DropColumn(name: "SeoTitle", table: "NewsArticles");
            migrationBuilder.DropColumn(name: "TitleEn", table: "NewsArticles");
            migrationBuilder.DropColumn(name: "TitleUa", table: "NewsArticles");
            migrationBuilder.DropColumn(name: "SeoDescription", table: "NavigationItems");
            migrationBuilder.DropColumn(name: "SeoKeywords", table: "NavigationItems");
            migrationBuilder.DropColumn(name: "SeoTitle", table: "NavigationItems");
            migrationBuilder.DropColumn(name: "TitleEn", table: "NavigationItems");
            migrationBuilder.DropColumn(name: "TitleUa", table: "NavigationItems");
            migrationBuilder.DropColumn(name: "DescriptionEn", table: "Distributors");
            migrationBuilder.DropColumn(name: "DescriptionUa", table: "Distributors");
            migrationBuilder.DropColumn(name: "Description", table: "Bands");
            migrationBuilder.DropColumn(name: "SeoDescription", table: "Bands");
            migrationBuilder.DropColumn(name: "SeoKeywords", table: "Bands");
            migrationBuilder.DropColumn(name: "SeoTitle", table: "Bands");
            migrationBuilder.DropColumn(name: "SeoDescription", table: "Albums");
            migrationBuilder.DropColumn(name: "SeoKeywords", table: "Albums");
            migrationBuilder.DropColumn(name: "SeoTitle", table: "Albums");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumTranslations");

            migrationBuilder.DropTable(
                name: "BandTranslations");

            migrationBuilder.DropTable(
                name: "DistributorTranslations");

            migrationBuilder.DropTable(
                name: "NavigationItemTranslations");

            migrationBuilder.DropTable(
                name: "NewsArticleTranslations");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.AddColumn<string>(
                name: "ContentEn",
                table: "NewsArticles",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "ContentUa",
                table: "NewsArticles",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "NewsArticles",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoKeywords",
                table: "NewsArticles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "NewsArticles",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "NewsArticles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "TitleUa",
                table: "NewsArticles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "NavigationItems",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoKeywords",
                table: "NavigationItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "NavigationItems",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "NavigationItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "TitleUa",
                table: "NavigationItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Distributors",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionUa",
                table: "Distributors",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Bands",
                type: "text",
                nullable: true);

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
        }
    }
}
