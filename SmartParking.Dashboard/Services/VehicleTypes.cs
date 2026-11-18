namespace SmartParking.Dashboard.Services;

/// <summary>مشخصات نمایشی هر نوع وسیله نقلیه. کلید همان <c>Vehicle.VehicleTypeName</c> پروژه اصلی است.</summary>
public sealed record VehicleTypeInfo(string Key, string Name, string Icon, string Color);

public static class VehicleTypes
{
    public static readonly IReadOnlyList<VehicleTypeInfo> All = new[]
    {
        new VehicleTypeInfo("Car", "خودرو سواری", "car", "#38BDF8"),
        new VehicleTypeInfo("Motorcycle", "موتورسیکلت", "bike", "#10B981"),
        new VehicleTypeInfo("Truck", "کامیون", "truck", "#F59E0B"),
        new VehicleTypeInfo("ElectricCar", "خودروی برقی", "bolt", "#A78BFA"),
        new VehicleTypeInfo("Bus", "اتوبوس", "bus", "#F472B6"),
    };

    private static readonly VehicleTypeInfo Unknown = new("?", "نامشخص", "car", "#94A3B8");

    public static VehicleTypeInfo Get(string key)
        => All.FirstOrDefault(t => t.Key == key) ?? Unknown;
}
