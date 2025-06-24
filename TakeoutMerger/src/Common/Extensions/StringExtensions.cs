using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakeoutMerger.src.Common.Extensions
{
    public static class StringExtensions
    {
        public static DateTime GetDateTimeFromTimestamp(this string timestamp)
        {
            if (DateTime.TryParse(timestamp, out var dateTime))
            {
                return dateTime;
            }

            if (long.TryParse(timestamp, out var unixTimestamp))
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
            }

            throw new FormatException($"Invalid timestamp format: {timestamp}");
        }

        public static DateTime GetDateTimeFromFormattedString(this string formattedString)
        {
            formattedString = formattedString.Replace("UTC", string.Empty).Trim();

            if (DateTime.TryParse(formattedString, out var dateTime))
            {
                return dateTime;
            }
            throw new FormatException($"Invalid formatted date string: {formattedString}");
        }
    }
}
