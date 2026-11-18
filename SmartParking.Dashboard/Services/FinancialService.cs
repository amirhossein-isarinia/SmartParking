using System.Text;
using SmartParking.Services;

namespace SmartParking.Dashboard.Services;

public sealed record ExitQuery(
    string? Search = null, string? TypeKey = null, DateTime? From = null, DateTime? To = null,
    string SortKey = "exit", bool Descending = true);

/// <summary>شماره‌ی ثبت (ترتیب خروج در نشست جاری) + رکورد واقعی ExitRecord.</summary>
public sealed record ExitRow(int Number, ExitRecord Record);

public sealed record TypeRevenue(string Key, string Name, string Color, decimal Revenue, int Count);

public sealed record DailyRevenue(DateTime Date, int Count, decimal Revenue);

public sealed record ExitTotals(decimal Revenue, int Count, decimal Average);

/// <summary>گزارش‌های مالی و تاریخچه؛ درآمد فقط از مجموع Fee رکوردهای واقعی خروج به‌دست می‌آید.</summary>
public sealed class FinancialService
{
    private readonly ParkingStateService _state;

    public FinancialService(ParkingStateService state)
    {
        _state = state;
    }

    public IReadOnlyList<ExitRow> Query(ExitQuery q)
    {
        var all = _state.Snapshot().Exits.Select((r, i) => new ExitRow(i + 1, r));
        IEnumerable<ExitRow> rows = all;

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            string plate = SmartParking.Models.Vehicle.NormalizePlate(Fa.NormalizeDigits(q.Search));
            rows = rows.Where(r => r.Record.Vehicle.LicensePlate.Contains(plate, StringComparison.Ordinal));
        }
        if (!string.IsNullOrEmpty(q.TypeKey))
            rows = rows.Where(r => r.Record.Vehicle.VehicleTypeName == q.TypeKey);
        if (q.From is DateTime from)
            rows = rows.Where(r => r.Record.ExitTime.Date >= from.Date);
        if (q.To is DateTime to)
            rows = rows.Where(r => r.Record.ExitTime.Date <= to.Date);

        Func<ExitRow, object> key = q.SortKey switch
        {
            "plate" => r => r.Record.Vehicle.LicensePlate,
            "owner" => r => r.Record.Vehicle.OwnerName,
            "type" => r => Fa.TypeName(r.Record.Vehicle.VehicleTypeName),
            "entry" => r => r.Record.Vehicle.EntryTime,
            "duration" => r => r.Record.Duration,
            "fee" => r => r.Record.Fee,
            _ => r => r.Record.ExitTime
        };

        return (q.Descending ? rows.OrderByDescending(key) : rows.OrderBy(key)).ToList();
    }

    public ExitTotals Totals(IEnumerable<ExitRow> rows)
    {
        var list = rows.ToList();
        decimal sum = list.Sum(r => r.Record.Fee);
        return new ExitTotals(sum, list.Count, list.Count == 0 ? 0 : Math.Round(sum / list.Count, 0, MidpointRounding.AwayFromZero));
    }

    public decimal TotalRevenue() => _state.Read(m => m.GetTotalRevenue());

    public decimal TodayRevenue() => _state.Read(m => m.GetRevenueByDate(DateTime.Today));

    public IReadOnlyList<TypeRevenue> RevenueByType(IEnumerable<ExitRow> rows)
    {
        var list = rows.ToList();
        return VehicleTypes.All.Select(t =>
        {
            var of = list.Where(r => r.Record.Vehicle.VehicleTypeName == t.Key).ToList();
            return new TypeRevenue(t.Key, t.Name, t.Color, of.Sum(r => r.Record.Fee), of.Count);
        }).ToList();
    }

    public IReadOnlyList<DailyRevenue> Daily(IEnumerable<ExitRow> rows)
        => rows.GroupBy(r => r.Record.ExitTime.Date)
               .OrderByDescending(g => g.Key)
               .Select(g => new DailyRevenue(g.Key, g.Count(), g.Sum(r => r.Record.Fee)))
               .ToList();

    // ---------- خروجی CSV (UTF-8 با BOM برای نمایش درست فارسی در Excel) ----------

    public string BuildCsv(IEnumerable<ExitRow> rows)
    {
        var sb = new StringBuilder("\uFEFF");
        sb.AppendLine(string.Join(",", new[]
        {
            "شماره", "پلاک", "مالک", "نوع وسیله", "VIP",
            "ورود (شمسی)", "ورود (میلادی)", "خروج (شمسی)", "خروج (میلادی)",
            "مدت توقف (دقیقه)", "مبلغ نهایی (تومان)"
        }));

        foreach (ExitRow row in rows)
        {
            var r = row.Record;
            var v = r.Vehicle;
            sb.AppendLine(string.Join(",", new[]
            {
                Cell(row.Number.ToString()),
                Cell(v.LicensePlate, text: true),
                Cell(v.OwnerName, text: true),
                Cell(Fa.TypeName(v.VehicleTypeName)),
                Cell(v.IsVip ? "VIP" : "عادی"),
                Cell(Stamp(v.EntryTime, jalali: true)), Cell(Stamp(v.EntryTime, jalali: false)),
                Cell(Stamp(r.ExitTime, jalali: true)), Cell(Stamp(r.ExitTime, jalali: false)),
                Cell(((long)r.Duration.TotalMinutes).ToString()),
                Cell(r.Fee.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))
            }));
        }
        return sb.ToString();
    }

    private static string Stamp(DateTime t, bool jalali)
        => jalali ? Fa.NormalizeDigits(Fa.Stamp(t)) : t.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);

    private static string Cell(string value, bool text = false)
    {
        // جلوگیری از اجرای فرمول در Excel برای مقادیر متنی واردشده توسط کاربر
        if (text && value.Length > 0 && "=+-@\t\r".Contains(value[0]))
            value = "'" + value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
