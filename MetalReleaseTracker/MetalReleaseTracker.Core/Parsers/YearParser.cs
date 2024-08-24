using System.Globalization;

namespace MetalReleaseTracker.Core.Parsers
{
    public class YearParser
    {
        public DateTime ParseYear(string year)
        {
            if (DateTime.TryParseExact(year?.Trim(), "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            return DateTime.MinValue;
        }
    }
}
