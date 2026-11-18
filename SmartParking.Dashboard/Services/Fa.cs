using System.Globalization;

namespace SmartParking.Dashboard.Services;

/// <summary>توابع قالب‌بندی فارسی: ارقام، پول، تاریخ شمسی، مدت زمان و نام نوع وسیله.</summary>
public static class Fa
{
    private static readonly PersianCalendar Calendar = new();
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private static readonly string[] MonthNames =
    {
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    };

    private static readonly Dictionary<DayOfWeek, string> DayNames = new()
    {
        [DayOfWeek.Saturday] = "شنبه",
        [DayOfWeek.Sunday] = "یکشنبه",
        [DayOfWeek.Monday] = "دوشنبه",
        [DayOfWeek.Tuesday] = "سه‌شنبه",
        [DayOfWeek.Wednesday] = "چهارشنبه",
        [DayOfWeek.Thursday] = "پنجشنبه",
        [DayOfWeek.Friday] = "جمعه"
    };

    /// <summary>تبدیل ارقام لاتین به فارسی (برای نمایش).</summary>
    public static string Digits(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        var chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
            if (chars[i] is >= '0' and <= '9')
                chars[i] = (char)('۰' + (chars[i] - '0'));
        return new string(chars);
    }

    /// <summary>تبدیل ارقام فارسی/عربی و جداکننده‌ها به لاتین (برای ورودی کاربر).</summary>
    public static string NormalizeDigits(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        var chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (c is >= '۰' and <= '۹') chars[i] = (char)('0' + (c - '۰'));
            else if (c is >= '٠' and <= '٩') chars[i] = (char)('0' + (c - '٠'));
            else if (c == '٫' || c == '،') chars[i] = '.';
        }
        return new string(chars).Replace("٬", string.Empty);
    }

    public static string Number(decimal value) => Digits(value.ToString("N0", Inv)).Replace(",", "٬");
    public static string Number(int value) => Number((decimal)value);

    public static string Decimal(decimal value)
        => Digits(value.ToString("#,##0.##", Inv)).Replace(",", "٬").Replace(".", "٫");

    public static string Money(decimal value) => Number(value) + " تومان";

    public static string Percent(double value, int decimals = 0)
        => Digits(value.ToString("F" + decimals, Inv)).Replace(".", "٫") + "٪";

    public static string Date(DateTime value)
        => Digits($"{Calendar.GetYear(value):0000}/{Calendar.GetMonth(value):00}/{Calendar.GetDayOfMonth(value):00}");

    public static string ShortDate(DateTime value)
        => Digits($"{Calendar.GetMonth(value):00}/{Calendar.GetDayOfMonth(value):00}");

    public static string LongDate(DateTime value)
        => $"{DayNames[Calendar.GetDayOfWeek(value)]}، {Digits(Calendar.GetDayOfMonth(value).ToString(Inv))} " +
           $"{MonthNames[Calendar.GetMonth(value) - 1]} {Digits(Calendar.GetYear(value).ToString(Inv))}";

    public static string Time(DateTime value) => Digits(value.ToString("HH:mm", Inv));
    public static string TimeWithSeconds(DateTime value) => Digits(value.ToString("HH:mm:ss", Inv));

    public static string Stamp(DateTime value) => $"{Date(value)}  {Time(value)}";

    public static string Duration(TimeSpan value)
    {
        if (value < TimeSpan.Zero) value = TimeSpan.Zero;
        int hours = (int)value.TotalHours;
        int minutes = value.Minutes;
        if (hours == 0) return $"{Digits(minutes.ToString(Inv))} دقیقه";
        if (minutes == 0) return $"{Digits(hours.ToString(Inv))} ساعت";
        return $"{Digits(hours.ToString(Inv))} ساعت و {Digits(minutes.ToString(Inv))} دقیقه";
    }

    /// <summary>مقدار قابل‌استفاده در input از نوع datetime-local.</summary>
    public static string ToInputValue(DateTime value) => value.ToString("yyyy-MM-ddTHH:mm", Inv);

    public static bool TryParseInputDateTime(string? text, out DateTime value)
        => DateTime.TryParseExact(text, "yyyy-MM-ddTHH:mm", Inv, DateTimeStyles.None, out value)
           || DateTime.TryParseExact(text, "yyyy-MM-ddTHH:mm:ss", Inv, DateTimeStyles.None, out value);

    public static bool TryParseInputDate(string? text, out DateTime value)
        => DateTime.TryParseExact(text, "yyyy-MM-dd", Inv, DateTimeStyles.None, out value);

    public static string ToInputDate(DateTime value) => value.ToString("yyyy-MM-dd", Inv);

    public static bool TryParseInt(string? text, out int value)
        => int.TryParse(NormalizeDigits(text).Trim(), NumberStyles.Integer, Inv, out value);

    public static bool TryParseDecimal(string? text, out decimal value)
        => decimal.TryParse(NormalizeDigits(text).Trim(), NumberStyles.Number, Inv, out value);

    public static string TypeName(string key) => VehicleTypes.Get(key).Name;
}
