using SmartParking.Models;

namespace SmartParking.Dashboard.Services;

/// <summary>مقادیر اختصاصی هر نوع وسیله (فقط مقدار مربوط به نوع انتخاب‌شده استفاده می‌شود).</summary>
public sealed record VehicleSpecs(int Doors = 4, int EngineCc = 150, decimal Tons = 1m, decimal Kwh = 50m, bool Charger = false, int Seats = 40);

/// <summary>
/// ساخت و کپی وسایل نقلیه برای لایه‌ی نمایش. اعتبارسنجی مقادیر کاملاً بر عهده‌ی سازنده‌ی کلاس‌های اصلی است
/// و هیچ فرمول هزینه‌ای اینجا وجود ندارد. این کلاس مشابه <c>CreateVehicle</c> در ConsoleMenu است.
/// </summary>
public static class VehicleFactory
{
    public static Vehicle Create(string typeKey, string plate, string owner, DateTime entryTime, bool isVip, VehicleSpecs s)
        => typeKey switch
        {
            "Car" => new Car(plate, owner, entryTime, s.Doors, isVip),
            "Motorcycle" => new Motorcycle(plate, owner, entryTime, s.EngineCc, isVip),
            "Truck" => new Truck(plate, owner, entryTime, s.Tons, isVip),
            "ElectricCar" => new ElectricCar(plate, owner, entryTime, s.Kwh, s.Charger, isVip),
            "Bus" => new Bus(plate, owner, entryTime, s.Seats, isVip),
            _ => throw new ArgumentException("Unknown vehicle type.")
        };

    /// <summary>
    /// نسخه‌ای از همان وسیله با VIP / شارژر متفاوت؛ برای تفکیک اجزای هزینه در پیش‌نمایش
    /// (خود مبلغ همیشه توسط <c>CalculateParkingFee</c> کلاس اصلی محاسبه می‌شود).
    /// </summary>
    public static Vehicle Clone(Vehicle v, bool? isVip = null, bool? usesCharger = null)
    {
        bool vip = isVip ?? v.IsVip;
        return v switch
        {
            Car c => new Car(c.LicensePlate, c.OwnerName, c.EntryTime, c.DoorCount, vip),
            Motorcycle m => new Motorcycle(m.LicensePlate, m.OwnerName, m.EntryTime, m.EngineCapacityCc, vip),
            Truck t => new Truck(t.LicensePlate, t.OwnerName, t.EntryTime, t.CapacityTons, vip),
            ElectricCar e => new ElectricCar(e.LicensePlate, e.OwnerName, e.EntryTime, e.BatteryCapacityKwh, usesCharger ?? e.UsesParkingCharger, vip),
            Bus b => new Bus(b.LicensePlate, b.OwnerName, b.EntryTime, b.SeatCount, vip),
            _ => throw new NotSupportedException("Unsupported vehicle type.")
        };
    }

    /// <summary>ویژگی‌های اختصاصی هر نوع برای نمایش در جزئیات.</summary>
    public static IReadOnlyList<(string Label, string Value)> Describe(Vehicle v) => v switch
    {
        Car c => new[] { ("تعداد درب", Fa.Number(c.DoorCount)) },
        Motorcycle m => new[] { ("حجم موتور", Fa.Number(m.EngineCapacityCc) + " سی‌سی") },
        Truck t => new[] { ("ظرفیت بار", Fa.Decimal(t.CapacityTons) + " تن") },
        ElectricCar e => new[]
        {
            ("ظرفیت باتری", Fa.Decimal(e.BatteryCapacityKwh) + " کیلووات‌ساعت"),
            ("شارژر پارکینگ", e.UsesParkingCharger ? "استفاده می‌کند" : "استفاده نمی‌کند")
        },
        Bus b => new[] { ("تعداد صندلی", Fa.Number(b.SeatCount)) },
        _ => Array.Empty<(string, string)>()
    };
}
