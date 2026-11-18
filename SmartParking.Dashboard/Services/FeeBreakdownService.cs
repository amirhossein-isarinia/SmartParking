using SmartParking.Models;

namespace SmartParking.Dashboard.Services;

public sealed record FeeBreakdown(
    string TypeKey,
    int BillableHours,
    decimal FirstHourFee,
    decimal AdditionalHourFee,
    decimal BaseFee,
    decimal TypeAdjustment,
    string? TypeAdjustmentLabel,
    bool IsVip,
    decimal VipAdjustment,
    bool UsedCharger,
    decimal ChargerFee,
    decimal Total);

/// <summary>
/// تفکیک اجزای هزینه برای پیش‌نمایش و رسید. مبلغ نهایی (<see cref="FeeBreakdown.Total"/>) همیشه نتیجه‌ی
/// مستقیم <c>Vehicle.CalculateParkingFee</c> است؛ اجزای تخفیف/هزینه اضافه از اختلاف خروجی همان متد
/// روی نسخه‌های «بدون VIP / بدون شارژر» به‌دست می‌آیند، نه از فرمول جداگانه.
/// </summary>
public sealed class FeeBreakdownService
{
    private readonly TariffCatalog _tariffs;

    public FeeBreakdownService(TariffCatalog tariffs)
    {
        _tariffs = tariffs;
    }

    public FeeBreakdown Calculate(Vehicle vehicle, DateTime exitTime)
    {
        decimal total = vehicle.CalculateParkingFee(exitTime);

        TimeSpan duration = vehicle.GetParkingDuration(exitTime);
        int hours = Math.Max(1, (int)Math.Ceiling(duration.TotalHours));
        TariffInfo tariff = _tariffs.Get(vehicle.VehicleTypeName);

        decimal baseFee = tariff.FirstHour + (hours - 1) * tariff.AdditionalHour;

        decimal plain = VehicleFactory.Clone(vehicle, isVip: false, usesCharger: false).CalculateParkingFee(exitTime);
        decimal typeAdjustment = plain - baseFee;

        decimal withVip = vehicle.IsVip
            ? VehicleFactory.Clone(vehicle, isVip: true, usesCharger: false).CalculateParkingFee(exitTime)
            : plain;
        decimal vipAdjustment = withVip - plain;

        decimal chargerFee = total - withVip;
        bool usedCharger = vehicle is ElectricCar { UsesParkingCharger: true };

        string? label = typeAdjustment < 0 ? tariff.DiscountLabel
                      : typeAdjustment > 0 ? tariff.SurchargeLabel
                      : null;

        return new FeeBreakdown(vehicle.VehicleTypeName, hours, tariff.FirstHour, tariff.AdditionalHour, baseFee,
            typeAdjustment, label, vehicle.IsVip, vipAdjustment, usedCharger, chargerFee, total);
    }
}
