using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO ""Reviews"" (""Id"", ""UserName"", ""Message"", ""CreatedDate"") VALUES
(gen_random_uuid(), 'metalfan_ua', 'Нарешті нормальний каталог! Раніше годинами шукав де купити вініл Drudkh, а тут все зібрано.', '2026-02-18T10:23:00Z'),
(gen_random_uuid(), 'vinyl_collector', 'super useful for tracking restocks. grabbed the Hate Forest LP before it sold out thanks to this site', '2026-02-19T14:05:00Z'),
(gen_random_uuid(), 'black_metal_maniac', 'Порівняння цін від різних дистро - це те що треба. Зекономив 8 євро на замовленні.', '2026-02-21T09:17:00Z'),
(gen_random_uuid(), 'Andriy K.', 'Користуюсь щодня. Єдине - хотілось би сповіщення коли з''являється новий реліз улюбленого гурту.', '2026-02-23T18:42:00Z'),
(gen_random_uuid(), 'tapeworm_zine', 'finally someone aggregated all the UA metal distros. bookmarked.', '2026-02-25T11:30:00Z'),
(gen_random_uuid(), 'Oleg', 'Дуже зручно що можна фільтрувати по формату. Збираю тільки касети і це прям спрощує пошук.', '2026-03-01T16:55:00Z'),
(gen_random_uuid(), 'nightside_records', 'good resource for keeping track of what''s available. would be cool to add bandcamp links too', '2026-03-04T08:20:00Z'),
(gen_random_uuid(), 'Марина', 'Підказала друзям, всі задоволені. Раніше навіть не знали що стільки наших гуртів продається за кордоном.', '2026-03-07T20:11:00Z'),
(gen_random_uuid(), 'grimkvlt', 'the calendar feature is a nice touch. preordered the new Windswept because of it', '2026-03-10T13:44:00Z'),
(gen_random_uuid(), 'Taras M.', 'Працює швидко, дизайн приємний. Одне зауваження - було б добре бачити дату додавання альбому в каталог.', '2026-03-14T07:38:00Z'),
(gen_random_uuid(), 'death_metal_dad', 'ordered 3 CDs this month that I found here. solid site, keep it up', '2026-03-17T22:09:00Z'),
(gen_random_uuid(), 'Svitlana R.', 'Подарувала чоловіку вініл 1914 який знайшла тут. Він був в захваті, каже раніше не міг знайти де його замовити.', '2026-03-20T15:26:00Z')
ON CONFLICT DO NOTHING;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""Reviews"" WHERE ""UserName"" IN ('metalfan_ua', 'vinyl_collector', 'black_metal_maniac', 'Andriy K.', 'tapeworm_zine', 'Oleg', 'nightside_records', 'Марина', 'grimkvlt', 'Taras M.', 'death_metal_dad', 'Svitlana R.');");
        }
    }
}
