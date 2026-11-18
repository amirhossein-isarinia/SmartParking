using SmartParking.Services;

namespace SmartParking.Dashboard.Services;

/// <summary>
/// تصویر لحظه‌ای از وضعیت پارکینگ. همه‌ی اعداد تجمیعی با متدهای خود ParkingManager محاسبه شده‌اند.
/// </summary>
public sealed record ParkingSnapshot(
    IReadOnlyList<SmartParking.Models.Vehicle> Parked,
    IReadOnlyList<ExitRecord> Exits,
    int Capacity,
    decimal TotalRevenue,
    IReadOnlyDictionary<string, int> ParkedByType,
    IReadOnlyDictionary<string, int> ExitedByType)
{
    public int ParkedCount => Parked.Count;
    public int FreeCount => Math.Max(0, Capacity - Parked.Count);
    public double OccupancyPercent => Capacity == 0 ? 0 : Parked.Count * 100.0 / Capacity;
    public bool IsFull => Parked.Count >= Capacity;
}

/// <summary>
/// تنها دروازه‌ی داشبورد به ParkingManager. یک نمونه‌ی مشترک (Singleton) دارد؛
/// تمام خواندن‌ها و نوشتن‌ها زیر قفل انجام می‌شود چون ParkingManager ذاتاً Thread-Safe نیست
/// و ممکن است چند تب مرورگر هم‌زمان استفاده شود. هر تغییر، رویداد Changed را برای به‌روزرسانی صفحات فعال می‌کند.
/// </summary>
public sealed class ParkingStateService
{
    private readonly ParkingManager _manager;
    private readonly object _gate = new();

    public ParkingStateService(ParkingManager manager)
    {
        _manager = manager;
    }

    public event Action? Changed;

    public int Capacity => ParkingManager.Capacity;

    public T Read<T>(Func<ParkingManager, T> reader)
    {
        lock (_gate) return reader(_manager);
    }

    public ParkingSnapshot Snapshot()
    {
        lock (_gate)
        {
            return new ParkingSnapshot(
                _manager.ParkedVehicles.ToList(),
                _manager.ExitHistory.ToList(),
                ParkingManager.Capacity,
                _manager.GetTotalRevenue(),
                new Dictionary<string, int>(_manager.GetParkedCountByType()),
                new Dictionary<string, int>(_manager.GetExitedCountByType()));
        }
    }

    /// <summary>شماره‌ی ثبت یک رکورد خروج (ترتیب آن در تاریخچه‌ی نشست جاری، از ۱).</summary>
    public int NumberOf(ExitRecord record)
        => Read(m => m.ExitHistory.ToList().IndexOf(record) + 1);

    public SmartParking.Models.Vehicle? Find(string? plate)
        => string.IsNullOrWhiteSpace(plate) ? null : Read(m => m.FindByLicensePlate(plate));

    /// <summary>ثبت ورود از طریق همان متد اصلی (بررسی ظرفیت و پلاک تکراری داخل ParkingManager است).</summary>
    public void RegisterEntry(SmartParking.Models.Vehicle vehicle)
    {
        lock (_gate) _manager.RegisterEntry(vehicle);
        Changed?.Invoke();
    }

    /// <summary>ثبت خروج از طریق همان متد اصلی؛ هزینه توسط خود وسیله نقلیه محاسبه می‌شود.</summary>
    public ExitRecord RegisterExit(string plate, DateTime exitTime)
    {
        ExitRecord record;
        lock (_gate) record = _manager.RegisterExit(plate, exitTime);
        Changed?.Invoke();
        return record;
    }
}
