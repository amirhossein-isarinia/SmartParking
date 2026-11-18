using SmartParking.Models;

namespace SmartParking.Dashboard.Services;

public sealed record TypeStat(string Key, string Name, string Color, int Entries, int Exits, decimal Revenue, TimeSpan? AverageStay);

public sealed record AnalyticsData(
    int TotalEntries, int TotalExits, int Parked, double Occupancy,
    TimeSpan? AverageStay, int VipVehicles, decimal TotalRevenue, decimal? AverageRevenue,
    IReadOnlyList<TypeStat> ByType, IReadOnlyList<ChartPoint> EntriesByHour);

/// <summary>شاخص‌های آماری؛ فقط از داده‌ی واقعی. اگر داده‌ای نباشد مقدار null برمی‌گردد (نه عدد تخمینی).</summary>
public sealed class AnalyticsService
{
    private readonly ParkingStateService _state;

    public AnalyticsService(ParkingStateService state)
    {
        _state = state;
    }

    public AnalyticsData Get()
    {
        ParkingSnapshot s = _state.Snapshot();

        var everyVehicle = s.Parked.Concat(s.Exits.Select(r => r.Vehicle)).ToList();
        int totalEntries = everyVehicle.Count;
        int vip = everyVehicle.Where(v => v.IsVip).Select(v => v.LicensePlate).Distinct().Count();

        TimeSpan? avgStay = s.Exits.Count == 0
            ? null
            : TimeSpan.FromTicks((long)s.Exits.Average(r => r.Duration.Ticks));

        decimal? avgRevenue = s.Exits.Count == 0 ? null : Math.Round(s.TotalRevenue / s.Exits.Count, 0, MidpointRounding.AwayFromZero);

        var byType = VehicleTypes.All.Select(t =>
        {
            var exits = s.Exits.Where(r => r.Vehicle.VehicleTypeName == t.Key).ToList();
            int entries = everyVehicle.Count(v => v.VehicleTypeName == t.Key);
            TimeSpan? stay = exits.Count == 0 ? null : TimeSpan.FromTicks((long)exits.Average(r => r.Duration.Ticks));
            return new TypeStat(t.Key, t.Name, t.Color, entries, exits.Count, exits.Sum(r => r.Fee), stay);
        }).ToList();

        var byHour = Enumerable.Range(0, 24).Select(h =>
        {
            int count = everyVehicle.Count(v => v.EntryTime.Hour == h);
            string label = Fa.Digits(h.ToString("00"));
            return new ChartPoint(label, count, $"ساعت {label}: {Fa.Number(count)} ورود");
        }).ToList();

        return new AnalyticsData(totalEntries, s.Exits.Count, s.ParkedCount, s.OccupancyPercent,
            avgStay, vip, s.TotalRevenue, avgRevenue, byType, byHour);
    }
}
