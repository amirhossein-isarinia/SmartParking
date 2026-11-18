using System.Globalization;

namespace SmartParking.UI;

/// <summary>توابع کمکی دریافت و اعتبارسنجی ورودی کنسول (جدا از منطق برنامه).</summary>
public static class InputHelper
{
    public const string DateTimeFormat = "yyyy-MM-dd HH:mm";
    public const string DateFormat = "yyyy-MM-dd";

    public static string ReadNonEmpty(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();
            Console.WriteLine("  Input cannot be empty. Try again.");
        }
    }

    public static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine()?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
                && value >= min && value <= max)
                return value;
            Console.WriteLine($"  Please enter a whole number between {min} and {max}.");
        }
    }

    public static decimal ReadDecimal(string prompt, decimal minExclusive, decimal max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (decimal.TryParse(Console.ReadLine()?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value)
                && value > minExclusive && value <= max)
                return value;
            Console.WriteLine($"  Please enter a number greater than {minExclusive} and at most {max}.");
        }
    }

    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt + " (y/n): ");
            string? input = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (input is "y" or "yes") return true;
            if (input is "n" or "no") return false;
            Console.WriteLine("  Please enter 'y' or 'n'.");
        }
    }

    /// <summary>زمان اختیاری؛ با Enter زمان فعلی برگردانده می‌شود.</summary>
    public static DateTime ReadOptionalDateTime(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} [{DateTimeFormat}, Enter = now]: ");
            string? input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
                return DateTime.Now;
            if (DateTime.TryParseExact(input, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return result;
            Console.WriteLine($"  Invalid format. Use {DateTimeFormat}.");
        }
    }

    public static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} [{DateFormat}, Enter = today]: ");
            string? input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
                return DateTime.Today;
            if (DateTime.TryParseExact(input, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return result;
            Console.WriteLine($"  Invalid format. Use {DateFormat}.");
        }
    }

    public static string FormatMoney(decimal amount)
        => amount.ToString("N0", CultureInfo.InvariantCulture) + " Toman";

    public static string FormatDuration(TimeSpan duration)
        => $"{(int)duration.TotalHours}h {duration.Minutes}m";
}
