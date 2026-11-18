namespace SmartParking.Models;

/// <summary>
/// کامیون: ساعت اول ۶۰٬۰۰۰ و هر ساعت اضافه ۴۰٬۰۰۰ تومان.
/// ظرفیت بیش از ۱۰ تن، ۲۰٪ هزینه اضافی دارد.
/// </summary>
public class Truck : Vehicle
{
    private const decimal HeavyTruckThresholdTons = 10m;
    private const decimal HeavyTruckSurchargeRate = 0.20m;

    private decimal _capacityTons;

    public Truck(string licensePlate, string ownerName, DateTime entryTime, decimal capacityTons, bool isVip = false)
        : base(licensePlate, ownerName, entryTime, isVip)
    {
        CapacityTons = capacityTons;
    }

    public decimal CapacityTons
    {
        get => _capacityTons;
        private set
        {
            if (value <= 0 || value > 100)
                throw new ArgumentException("Truck capacity must be greater than 0 and at most 100 tons.");
            _capacityTons = value;
        }
    }

    public override string VehicleTypeName => "Truck";
    protected override decimal FirstHourFee => 60_000m;
    protected override decimal AdditionalHourFee => 40_000m;

    public override decimal CalculateParkingFee(DateTime exitTime)
    {
        decimal fee = CalculateStandardFee(CalculateBillableHours(exitTime));

        if (CapacityTons > HeavyTruckThresholdTons)
            fee *= (1 + HeavyTruckSurchargeRate);

        return ApplyVipDiscount(fee);
    }

    protected override string GetExtraInfo() => $"Capacity: {CapacityTons} ton";
}
