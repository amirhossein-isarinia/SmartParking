using SmartParking.Models;

namespace SmartParking.Dashboard.Services;

public sealed record ParkedQuery(
    string? Search = null, string? TypeKey = null, bool? Vip = null,
    string SortKey = "entry", bool Descending = false);

public sealed record ParkedRow(int Number, Vehicle Vehicle, TimeSpan Duration);

public sealed record PlateSearchResult(Vehicle? Exact, IReadOnlyList<Vehicle> Similar, IReadOnlyList<ExitRow> History);

/// <summary>جست‌وجو، فیلتر و مرتب‌سازی وسایل حاضر (فقط روی داده‌ی واقعی ParkingManager).</summary>
public sealed class VehicleService
{
    private readonly ParkingStateService _state;

    public VehicleService(ParkingStateService state)
    {
        _state = state;
    }

    public IReadOnlyList<ParkedRow> QueryParked(ParkedQuery q)
    {
        DateTime now = DateTime.Now;
        IEnumerable<Vehicle> items = _state.Snapshot().Parked;

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            string needle = Fa.NormalizeDigits(q.Search).Trim();
            string plateNeedle = Vehicle.NormalizePlate(needle);
            items = items.Where(v =>
                v.LicensePlate.Contains(plateNeedle, StringComparison.Ordinal) ||
                v.OwnerName.Contains(needle, StringComparison.CurrentCultureIgnoreCase) ||
                Fa.NormalizeDigits(v.OwnerName).Contains(needle, StringComparison.CurrentCultureIgnoreCase));
        }

        if (!string.IsNullOrEmpty(q.TypeKey))
            items = items.Where(v => v.VehicleTypeName == q.TypeKey);
        if (q.Vip is bool vip)
            items = items.Where(v => v.IsVip == vip);

        Func<Vehicle, object> key = q.SortKey switch
        {
            "plate" => v => v.LicensePlate,
            "owner" => v => v.OwnerName,
            "type" => v => Fa.TypeName(v.VehicleTypeName),
            "vip" => v => v.IsVip,
            _ => v => v.EntryTime
        };

        var ordered = q.Descending ? items.OrderByDescending(key) : items.OrderBy(key);
        return ordered.Select((v, i) => new ParkedRow(i + 1, v, now > v.EntryTime ? now - v.EntryTime : TimeSpan.Zero)).ToList();
    }

    /// <summary>
    /// جست‌وجوی پلاک: مطابقت دقیق با وسایل حاضر از طریق متد اصلی FindByLicensePlate،
    /// به‌علاوه‌ی پلاک‌های مشابه و سوابق خروج قبلی همان پلاک.
    /// </summary>
    public PlateSearchResult SearchPlate(string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return new PlateSearchResult(null, Array.Empty<Vehicle>(), Array.Empty<ExitRow>());

        string plate = Vehicle.NormalizePlate(Fa.NormalizeDigits(term));
        Vehicle? exact = _state.Find(plate);
        var snap = _state.Snapshot();

        var similar = snap.Parked
            .Where(v => v.LicensePlate != exact?.LicensePlate && v.LicensePlate.Contains(plate, StringComparison.Ordinal))
            .ToList();

        var history = snap.Exits
            .Select((r, i) => new ExitRow(i + 1, r))
            .Where(r => r.Record.Vehicle.LicensePlate.Contains(plate, StringComparison.Ordinal))
            .OrderByDescending(r => r.Record.ExitTime)
            .ToList();

        return new PlateSearchResult(exact, similar, history);
    }
}
