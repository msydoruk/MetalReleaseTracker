using System.Text;
using System.Text.RegularExpressions;

namespace MetalReleaseTracker.CoreDataService.Services.Utilities;

public static partial class SlugGenerator
{
    private const int MaxSlugLength = 200;

    private static readonly Dictionary<char, string> TransliterationMap = new()
    {
        // Ukrainian Cyrillic → Latin (KMU 2010 standard)
        { 'А', "A" }, { 'а', "a" },
        { 'Б', "B" }, { 'б', "b" },
        { 'В', "V" }, { 'в', "v" },
        { 'Г', "H" }, { 'г', "h" },
        { 'Ґ', "G" }, { 'ґ', "g" },
        { 'Д', "D" }, { 'д', "d" },
        { 'Е', "E" }, { 'е', "e" },
        { 'Є', "Ye" }, { 'є', "ie" },
        { 'Ж', "Zh" }, { 'ж', "zh" },
        { 'З', "Z" }, { 'з', "z" },
        { 'И', "Y" }, { 'и', "y" },
        { 'І', "I" }, { 'і', "i" },
        { 'Ї', "Yi" }, { 'ї', "i" },
        { 'Й', "Y" }, { 'й', "i" },
        { 'К', "K" }, { 'к', "k" },
        { 'Л', "L" }, { 'л', "l" },
        { 'М', "M" }, { 'м', "m" },
        { 'Н', "N" }, { 'н', "n" },
        { 'О', "O" }, { 'о', "o" },
        { 'П', "P" }, { 'п', "p" },
        { 'Р', "R" }, { 'р', "r" },
        { 'С', "S" }, { 'с', "s" },
        { 'Т', "T" }, { 'т', "t" },
        { 'У', "U" }, { 'у', "u" },
        { 'Ф', "F" }, { 'ф', "f" },
        { 'Х', "Kh" }, { 'х', "kh" },
        { 'Ц', "Ts" }, { 'ц', "ts" },
        { 'Ч', "Ch" }, { 'ч', "ch" },
        { 'Ш', "Sh" }, { 'ш', "sh" },
        { 'Щ', "Shch" }, { 'щ', "shch" },
        { 'Ь', string.Empty }, { 'ь', string.Empty },
        { 'Ю', "Yu" }, { 'ю', "iu" },
        { 'Я', "Ya" }, { 'я', "ia" },
        { 'Ъ', string.Empty }, { 'ъ', string.Empty },
        { 'Ы', "Y" }, { 'ы', "y" },
        { 'Э', "E" }, { 'э', "e" },

        // Common diacritical characters from European languages
        { 'ä', "ae" }, { 'Ä', "Ae" },
        { 'ö', "oe" }, { 'Ö', "Oe" },
        { 'ü', "ue" }, { 'Ü', "Ue" },
        { 'ß', "ss" },
        { 'ø', "o" }, { 'Ø', "O" },
        { 'å', "a" }, { 'Å', "A" },
        { 'æ', "ae" }, { 'Æ', "Ae" },
        { 'ñ', "n" }, { 'Ñ', "N" },
        { 'ç', "c" }, { 'Ç', "C" },
        { 'ł', "l" }, { 'Ł', "L" },
        { 'ą', "a" }, { 'Ą', "A" },
        { 'ę', "e" }, { 'Ę', "E" },
        { 'ś', "s" }, { 'Ś', "S" },
        { 'ź', "z" }, { 'Ź', "Z" },
        { 'ż', "z" }, { 'Ż', "Z" },
        { 'ć', "c" }, { 'Ć', "C" },
        { 'ń', "n" }, { 'Ń', "N" },
        { 'é', "e" }, { 'É', "E" },
        { 'è', "e" }, { 'È', "E" },
        { 'ê', "e" }, { 'Ê', "E" },
        { 'ë', "e" }, { 'Ë', "E" },
        { 'á', "a" }, { 'Á', "A" },
        { 'à', "a" }, { 'À', "A" },
        { 'â', "a" }, { 'Â', "A" },
        { 'ã', "a" }, { 'Ã', "A" },
        { 'í', "i" }, { 'Í', "I" },
        { 'ì', "i" }, { 'Ì', "I" },
        { 'î', "i" }, { 'Î', "I" },
        { 'ï', "i" }, { 'Ï', "I" },
        { 'ó', "o" }, { 'Ó', "O" },
        { 'ò', "o" }, { 'Ò', "O" },
        { 'ô', "o" }, { 'Ô', "O" },
        { 'õ', "o" }, { 'Õ', "O" },
        { 'ú', "u" }, { 'Ú', "U" },
        { 'ù', "u" }, { 'Ù', "U" },
        { 'û', "u" }, { 'Û', "U" },
    };

    public static string GenerateSlug(string bandName, string albumName)
    {
        if (string.IsNullOrWhiteSpace(bandName) && string.IsNullOrWhiteSpace(albumName))
        {
            return string.Empty;
        }

        var combined = string.IsNullOrWhiteSpace(albumName)
            ? bandName.Trim()
            : $"{bandName.Trim()}-{albumName.Trim()}";

        return Slugify(Transliterate(combined));
    }

    public static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        return Slugify(Transliterate(name.Trim()));
    }

    private static string Transliterate(string text)
    {
        var result = new StringBuilder(text.Length * 2);

        foreach (var character in text)
        {
            if (TransliterationMap.TryGetValue(character, out var replacement))
            {
                result.Append(replacement);
            }
            else
            {
                result.Append(character);
            }
        }

        return result.ToString();
    }

    private static string Slugify(string text)
    {
        var slug = text.ToLowerInvariant();

        slug = NonAlphanumericRegex().Replace(slug, "-");

        slug = ConsecutiveHyphensRegex().Replace(slug, "-");

        slug = slug.Trim('-');

        if (slug.Length > MaxSlugLength)
        {
            slug = slug[..MaxSlugLength].TrimEnd('-');
        }

        return slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex("-{2,}")]
    private static partial Regex ConsecutiveHyphensRegex();
}
