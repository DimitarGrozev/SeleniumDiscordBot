using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Discordian.Core.Models.Discord
{
    public readonly partial record struct Snowflake(ulong Value)
    {
        public DateTimeOffset ToDate() => DateTimeOffset.FromUnixTimeMilliseconds(
            (long)((Value >> 22) + 1420070400000UL)
        ).ToLocalTime();

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    public partial record struct Snowflake
    {
        public static Snowflake Zero { get; } = new(0);

        public static Snowflake FromDate(DateTimeOffset date) => new Snowflake(
            ((ulong)date.ToUnixTimeMilliseconds() - 1420070400000UL) << 22
        );

        public static Snowflake? TryParse(string? str, IFormatProvider? formatProvider = null)
        {
            if (string.IsNullOrWhiteSpace(str))
                return null;

            // As number
            if (Regex.IsMatch(str, @"^\d+$") && ulong.TryParse(str, NumberStyles.Number, formatProvider, out var value))
            {
                return new Snowflake(value);
            }

            // As date
            if (DateTimeOffset.TryParse(str, formatProvider, DateTimeStyles.None, out var date))
            {
                return FromDate(date);
            }

            return null;
        }

        public static Snowflake Parse(string str, IFormatProvider? formatProvider) =>
            TryParse(str, formatProvider) ?? throw new FormatException($"Invalid snowflake '{str}'.");

        public static Snowflake Parse(string str) => Parse(str, null);
    }
}