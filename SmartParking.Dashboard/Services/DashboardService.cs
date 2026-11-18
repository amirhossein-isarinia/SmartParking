using SmartParking.Models;
using SmartParking.Services;

namespace SmartParking.Dashboard.Services;

public enum CapacityLevel { Normal, Warning, Critical }

public enum RevenueRange { Today, Week, Month, Custom }

public sealed record DashboardSummary(
    int Parked, int Capacity, int Free, double Occupancy,
    decimal Revenue, int Exits, CapacityLevel Level, bool IsFull);

public sealed record ChartPoint(string Label, decimal Value, string Tooltip);

public sealed record RevenueSeries(IReadOnlyList<ChartPoint> Points, decimal Total, int ExitCount, string? Error);

public sealed record TypeSlice(string Key, string Name, int Count, string Color);

public sealed record ActivityItem(Vehicle Vehicle, bool IsEntry, DateTime Time, decimal? Fee);

public static class CapacityLevels
{
    // آستانه‌های رنگ فقط برای نمایش هستند و روی قوانین ظرفیت (ParkingManager.Capacity) اثری ندارند.
    public const double WarningPercent = 70;
    public const double CriticalPercent = 90;

    public static CapacityLevel Of(double occupancyPercent, bool isFull)
        => isFull || occupancyPercent >= CriticalPercent ? CapacityLevel.Critical
         : occupancyPercent >= WarningPercent ? CapacityLevel.Warning
         : CapacityLevel.Normal;

    public static string Label(CapacityLevel level) => level switch
    {
        CapacityLevel.Critical => "ظرفیت تقریباً پر",
        CapacityLevel.Warning => "نزدیک به تکمیل",
        _ => "ظرفیت عادی"
    };

    public static string Css(CapacityLevel level) => level switch
    {
        CapacityLevel.Critical => "danger",
        CapacityLevel.Warning => "warn",
        _ => "ok"
    };
}

/// <summary>داده‌های صفحه‌ی داشبورد اصلی؛ همه از وضعیت واقعی ParkingManager.</summary>
public sealed class DashboardService
{
    public const int MaxCustomRangeDays = 92;

    private readonly ParkingStateService _state;

    public DashboardService(ParkingStateService state)
    {
        _state = state;
    }

    public DashboardSummary GetSummary()
    {
        ParkingSnapshot s = _state.Snapshot();
        return new DashboardSummary(
            s.ParkedCount, s.Capacity, s.FreeCount, s.OccupancyPercent,
            s.TotalRevenue, s.Exits.Count,
            CapacityLevels.Of(s.OccupancyPercent, s.IsFull), s.IsFull);
    }

    public RevenueSeries GetRevenueSeries(RevenueRange range, DateTime? customFrom, DateTime? customTo)
    {
        ParkingSnapshot snap = _state.Snapshot();
        DateTime today = DateTime.Today;

        if (range == RevenueRange.Today)
        {
            var todays = snap.Exits.Where(r => r.ExitTime.Date == today).ToList();
            var points = Enumerable.Range(0, 24).Select(h =>
            {
                var inHour = todays.Where(r => r.ExitTime.Hour == h).ToList();
                decimal sum = inHour.Sum(r => r.Fee);
                string label = Fa.Digits(h.ToString("00"));
                string tip = $"ساعت {label}: {Fa.Money(sum)} ({Fa.Number(inHour.Count)} خروج)";
                return new ChartPoint(label, sum, tip);
            }).ToList();
            return new RevenueSeries(points, todays.Sum(r => r.Fee), todays.Count, null);
        }

        DateTime from, to;
        switch (range)
        {
            case RevenueRange.Week: to = today; from = today.AddDays(-6); break;
            case RevenueRange.Month: to = today; from = today.AddDays(-29); break;
            default:
                if (customFrom is null || customTo is null)
                    return new RevenueSeries(Array.Empty<ChartPoint>(), 0, 0, "بازه‌ی تاریخ را کامل انتخاب کنید.");
                from = customFrom.Value.Date; to = customTo.Value.Date;
                if (to < from)
                    return new RevenueSeries(Array.Empty<ChartPoint>(), 0, 0, "تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد.");
                if ((to - from).TotalDays + 1 > MaxCustomRangeDays)
                    return new RevenueSeries(Array.Empty<ChartPoint>(), 0, 0,
                        $"حداکثر بازه‌ی قابل نمایش {Fa.Number(MaxCustomRangeDays)} روز است.");
                break;
        }

        var byDay = snap.Exits.Where(r => r.ExitTime.Date >= from && r.ExitTime.Date <= to)
                              .GroupBy(r => r.ExitTime.Date)
                              .ToDictionary(g => g.Key, g => (Sum: g.Sum(r => r.Fee), Count: g.Count()));

        var series = new List<ChartPoint>();
        for (DateTime d = from; d <= to; d = d.AddDays(1))
        {
            byDay.TryGetValue(d, out var v);
            series.Add(new ChartPoint(Fa.ShortDate(d), v.Sum,
                $"{Fa.LongDate(d)}: {Fa.Money(v.Sum)} ({Fa.Number(v.Count)} خروج)"));
        }

        return new RevenueSeries(series, byDay.Values.Sum(v => v.Sum), byDay.Values.Sum(v => v.Count), null);
    }

    public IReadOnlyList<TypeSlice> GetTypeMix(bool parked)
    {
        ParkingSnapshot s = _state.Snapshot();
        var counts = parked ? s.ParkedByType : s.ExitedByType;
        return VehicleTypes.All
            .Select(t => new TypeSlice(t.Key, t.Name, counts.TryGetValue(t.Key, out int c) ? c : 0, t.Color))
            .ToList();
    }

    public IReadOnlyList<ActivityItem> GetRecentActivities(int take)
    {
        ParkingSnapshot s = _state.Snapshot();
        var items = new List<ActivityItem>();
        items.AddRange(s.Parked.Select(v => new ActivityItem(v, true, v.EntryTime, null)));
        foreach (ExitRecord r in s.Exits)
        {
            items.Add(new ActivityItem(r.Vehicle, true, r.Vehicle.EntryTime, null));
            items.Add(new ActivityItem(r.Vehicle, false, r.ExitTime, r.Fee));
        }

        return items.OrderByDescending(i => i.Time)
                    .ThenBy(i => i.IsEntry ? 1 : 0)
                    .Take(take)
                    .ToList();
    }
}
