using SmartParking.Models;

namespace SmartParking.Services;

/// <summary>
/// مدیریت پارکینگ. این کلاس هیچ اطلاعی از انواع وسایل نقلیه ندارد و فقط با Vehicle کار می‌کند؛
/// پس افزودن نوع جدید (مثل Bus) نیازی به تغییر آن ندارد.
/// </summary>
public class ParkingManager
{
    public const int Capacity = 50;

    private readonly List<Vehicle> _parkedVehicles = new();
    private readonly List<ExitRecord> _exitHistory = new();

    public int ParkedCount => _parkedVehicles.Count;
    public bool IsFull => _parkedVehicles.Count >= Capacity;

    public IReadOnlyList<Vehicle> ParkedVehicles => _parkedVehicles.AsReadOnly();
    public IReadOnlyList<ExitRecord> ExitHistory => _exitHistory.AsReadOnly();

    /// <summary>ثبت ورود. در صورت پر بودن ظرفیت یا تکراری بودن پلاک خطا می‌دهد.</summary>
    public void RegisterEntry(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        if (IsFull)
            throw new InvalidOperationException($"Parking is full (capacity: {Capacity}). Entry is not allowed.");

        if (FindByLicensePlate(vehicle.LicensePlate) is not null)
            throw new InvalidOperationException($"A vehicle with plate '{vehicle.LicensePlate}' is already parked.");

        _parkedVehicles.Add(vehicle);
    }

    /// <summary>ثبت خروج، محاسبه هزینه (به‌صورت چندریخت) و حذف از فهرست حاضرین.</summary>
    public ExitRecord RegisterExit(string licensePlate, DateTime exitTime)
    {
        Vehicle vehicle = FindByLicensePlate(licensePlate)
            ?? throw new InvalidOperationException($"No parked vehicle found with plate '{Vehicle.NormalizePlate(licensePlate)}'.");

        // فراخوانی چندریخت؛ بدون if/switch روی نوع شیء. اگر زمان نامعتبر باشد، قبل از حذف خطا می‌دهد.
        decimal fee = vehicle.CalculateParkingFee(exitTime);

        var record = new ExitRecord(vehicle, exitTime, fee);
        _parkedVehicles.Remove(vehicle);
        _exitHistory.Add(record);
        return record;
    }

    public Vehicle? FindByLicensePlate(string licensePlate)
    {
        string normalized = Vehicle.NormalizePlate(licensePlate);
        return _parkedVehicles.FirstOrDefault(v => v.LicensePlate == normalized);
    }

    /// <summary>مجموع درآمد تمام خروج‌های ثبت‌شده.</summary>
    public decimal GetTotalRevenue() => _exitHistory.Sum(r => r.Fee);

    /// <summary>تعداد وسایل نقلیه حاضر به تفکیک نوع.</summary>
    public IReadOnlyDictionary<string, int> GetParkedCountByType()
        => _parkedVehicles
            .GroupBy(v => v.VehicleTypeName)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());

    /// <summary>تعداد وسایل نقلیه خارج‌شده به تفکیک نوع.</summary>
    public IReadOnlyDictionary<string, int> GetExitedCountByType()
        => _exitHistory
            .GroupBy(r => r.Vehicle.VehicleTypeName)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());

    /// <summary>چالش سوم: درآمد یک روز مشخص بر اساس تاریخ خروج.</summary>
    public decimal GetRevenueByDate(DateTime date)
        => _exitHistory.Where(r => r.ExitTime.Date == date.Date).Sum(r => r.Fee);

    /// <summary>سوابق خروج یک روز مشخص.</summary>
    public IReadOnlyList<ExitRecord> GetExitRecordsByDate(DateTime date)
        => _exitHistory.Where(r => r.ExitTime.Date == date.Date).ToList();
}
