namespace SmartParking.Models;

/// <summary>خودرو سواری: ساعت اول ۳۰٬۰۰۰ و هر ساعت اضافه ۲۰٬۰۰۰ تومان.</summary>
public class Car : Vehicle
{
    private int _doorCount;

    public Car(string licensePlate, string ownerName, DateTime entryTime, int doorCount = 4, bool isVip = false)
        : base(licensePlate, ownerName, entryTime, isVip)
    {
        DoorCount = doorCount;
    }

    public int DoorCount
    {
        get => _doorCount;
        private set
        {
            if (value < 2 || value > 5)
                throw new ArgumentException("Door count must be between 2 and 5.");
            _doorCount = value;
        }
    }

    public override string VehicleTypeName => "Car";
    protected override decimal FirstHourFee => 30_000m;
    protected override decimal AdditionalHourFee => 20_000m;

    public override decimal CalculateParkingFee(DateTime exitTime)
    {
        decimal fee = CalculateStandardFee(CalculateBillableHours(exitTime));
        return ApplyVipDiscount(fee);
    }

    protected override string GetExtraInfo() => $"Doors: {DoorCount}";
}
