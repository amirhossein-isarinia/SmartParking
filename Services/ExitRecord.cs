using SmartParking.Models;

namespace SmartParking.Services;

/// <summary>سابقه خروج یک وسیله (چالش سوم: گزارش تاریخی). غیرقابل تغییر است.</summary>
public sealed class ExitRecord
{
    public ExitRecord(Vehicle vehicle, DateTime exitTime, decimal fee)
    {
        Vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
        ExitTime = exitTime;
        Fee = fee;
    }

    public Vehicle Vehicle { get; }
    public DateTime ExitTime { get; }
    public decimal Fee { get; }
    public TimeSpan Duration => Vehicle.GetParkingDuration(ExitTime);
}
