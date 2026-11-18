namespace SmartParking.Models;

/// <summary>
/// خودرو برقی: ساعت اول ۲۰٬۰۰۰ و هر ساعت اضافه ۱۵٬۰۰۰ تومان.
/// در صورت استفاده از شارژر، ۵۰٬۰۰۰ تومان هزینه ثابت شارژ.
/// تصمیم طراحی: چون تعرفه‌اش با Car متفاوت است، مستقیماً از Vehicle ارث می‌برد
/// تا ویژگی‌ها و قوانین Car (مثل تعداد درب) به آن تحمیل نشود.
/// </summary>
public class ElectricCar : Vehicle
{
    private const decimal ChargingFee = 50_000m;

    private decimal _batteryCapacityKwh;

    public ElectricCar(string licensePlate, string ownerName, DateTime entryTime,
                       decimal batteryCapacityKwh, bool usesParkingCharger, bool isVip = false)
        : base(licensePlate, ownerName, entryTime, isVip)
    {
        BatteryCapacityKwh = batteryCapacityKwh;
        UsesParkingCharger = usesParkingCharger;
    }

    public decimal BatteryCapacityKwh
    {
        get => _batteryCapacityKwh;
        private set
        {
            if (value <= 0 || value > 300)
                throw new ArgumentException("Battery capacity must be greater than 0 and at most 300 kWh.");
            _batteryCapacityKwh = value;
        }
    }

    public bool UsesParkingCharger { get; }

    public override string VehicleTypeName => "ElectricCar";
    protected override decimal FirstHourFee => 20_000m;
    protected override decimal AdditionalHourFee => 15_000m;

    public override decimal CalculateParkingFee(DateTime exitTime)
    {
        // تخفیف VIP فقط روی هزینه توقف اعمال می‌شود؛ هزینه ثابت شارژ جداگانه اضافه می‌گردد.
        decimal parkingFee = ApplyVipDiscount(CalculateStandardFee(CalculateBillableHours(exitTime)));
        return UsesParkingCharger ? parkingFee + ChargingFee : parkingFee;
    }

    protected override string GetExtraInfo()
        => $"Battery: {BatteryCapacityKwh}kWh, Charger: {(UsesParkingCharger ? "Yes" : "No")}";
}
