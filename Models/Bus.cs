namespace SmartParking.Models;

/// <summary>
/// چالش دوم: اتوبوس. ساعت اول ۸۰٬۰۰۰ و هر ساعت اضافه ۵۰٬۰۰۰ تومان.
/// افزودن این کلاس نیازی به تغییر ParkingManager ندارد.
/// </summary>
public class Bus : Vehicle
{
    private int _seatCount;

    public Bus(string licensePlate, string ownerName, DateTime entryTime, int seatCount, bool isVip = false)
        : base(licensePlate, ownerName, entryTime, isVip)
    {
        SeatCount = seatCount;
    }

    public int SeatCount
    {
        get => _seatCount;
        private set
        {
            if (value < 10 || value > 100)
                throw new ArgumentException("Seat count must be between 10 and 100.");
            _seatCount = value;
        }
    }

    public override string VehicleTypeName => "Bus";
    protected override decimal FirstHourFee => 80_000m;
    protected override decimal AdditionalHourFee => 50_000m;

    public override decimal CalculateParkingFee(DateTime exitTime)
    {
        decimal fee = CalculateStandardFee(CalculateBillableHours(exitTime));
        return ApplyVipDiscount(fee);
    }

    protected override string GetExtraInfo() => $"Seats: {SeatCount}";
}
