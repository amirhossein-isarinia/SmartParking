namespace SmartParking.Models;

/// <summary>
/// موتورسیکلت: ساعت اول ۱۵٬۰۰۰ و هر ساعت اضافه ۱۰٬۰۰۰ تومان.
/// توقف بیش از ۵ ساعت ۱۰٪ تخفیف دارد.
/// </summary>
public class Motorcycle : Vehicle
{
    private const int LongStayThresholdHours = 5;
    private const decimal LongStayDiscountRate = 0.10m;

    private int _engineCapacityCc;

    public Motorcycle(string licensePlate, string ownerName, DateTime entryTime, int engineCapacityCc = 150, bool isVip = false)
        : base(licensePlate, ownerName, entryTime, isVip)
    {
        EngineCapacityCc = engineCapacityCc;
    }

    public int EngineCapacityCc
    {
        get => _engineCapacityCc;
        private set
        {
            if (value < 50 || value > 2000)
                throw new ArgumentException("Engine capacity must be between 50 and 2000 cc.");
            _engineCapacityCc = value;
        }
    }

    public override string VehicleTypeName => "Motorcycle";
    protected override decimal FirstHourFee => 15_000m;
    protected override decimal AdditionalHourFee => 10_000m;

    public override decimal CalculateParkingFee(DateTime exitTime)
    {
        int hours = CalculateBillableHours(exitTime);
        decimal fee = CalculateStandardFee(hours);

        // ساعت قابل پرداخت (گرد به بالا) بیشتر از ۵ یعنی توقف واقعی بیش از ۵ ساعت بوده است.
        if (hours > LongStayThresholdHours)
            fee *= (1 - LongStayDiscountRate);

        return ApplyVipDiscount(fee);
    }

    protected override string GetExtraInfo() => $"Engine: {EngineCapacityCc}cc";
}
