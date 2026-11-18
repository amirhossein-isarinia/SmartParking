namespace SmartParking.Models;

/// <summary>
/// کلاس پایه (انتزاعی) برای تمام وسایل نقلیه پارکینگ.
/// اطلاعات مشترک و منطق مشترک محاسبه (ساعت قابل پرداخت، فرمول پایه، تخفیف VIP) فقط یک بار اینجا نوشته شده است.
/// </summary>
public abstract class Vehicle
{
    /// <summary>درصد تخفیف اشتراک VIP (چالش اول).</summary>
    public const decimal VipDiscountRate = 0.30m;

    private string _licensePlate = string.Empty;
    private string _ownerName = string.Empty;

    protected Vehicle(string licensePlate, string ownerName, DateTime entryTime, bool isVip = false)
    {
        LicensePlate = licensePlate;
        OwnerName = ownerName;
        EntryTime = entryTime;
        IsVip = isVip;
    }

    // ---------- ویژگی‌های مشترک (کپسوله‌سازی + اعتبارسنجی) ----------

    public string LicensePlate
    {
        get => _licensePlate;
        private set
        {
            string normalized = NormalizePlate(value);
            if (normalized.Length < 3 || normalized.Length > 20)
                throw new ArgumentException("License plate must be between 3 and 20 characters.");
            if (!normalized.All(c => char.IsLetterOrDigit(c) || c == '-' || c == ' '))
                throw new ArgumentException("License plate may only contain letters, digits, '-' and spaces.");
            _licensePlate = normalized;
        }
    }

    public string OwnerName
    {
        get => _ownerName;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Owner name cannot be empty.");
            _ownerName = value.Trim();
        }
    }

    public DateTime EntryTime { get; }

    public bool IsVip { get; }

    // ---------- اعضای انتزاعی که هر فرزند باید تعریف کند ----------

    /// <summary>نام نوع وسیله (برای نمایش و آمار؛ بدون نیاز به بررسی نوع شیء).</summary>
    public abstract string VehicleTypeName { get; }

    /// <summary>هزینه ساعت اول (تومان).</summary>
    protected abstract decimal FirstHourFee { get; }

    /// <summary>هزینه هر ساعت اضافه (تومان).</summary>
    protected abstract decimal AdditionalHourFee { get; }

    /// <summary>
    /// محاسبه هزینه نهایی پارکینگ. متد انتزاعی است و هر کلاس فرزند طبق قوانین خودش آن را override می‌کند.
    /// </summary>
    public abstract decimal CalculateParkingFee(DateTime exitTime);

    // ---------- منطق مشترک (محافظت‌شده) ----------

    /// <summary>مدت توقف؛ اگر زمان خروج قبل از ورود باشد خطا می‌دهد.</summary>
    public TimeSpan GetParkingDuration(DateTime exitTime)
    {
        if (exitTime < EntryTime)
            throw new ArgumentException("Exit time cannot be earlier than entry time.");
        return exitTime - EntryTime;
    }

    /// <summary>ساعت قابل پرداخت: گرد به بالا و حداقل یک ساعت.</summary>
    protected int CalculateBillableHours(DateTime exitTime)
    {
        TimeSpan duration = GetParkingDuration(exitTime);
        int hours = (int)Math.Ceiling(duration.TotalHours);
        return Math.Max(1, hours);
    }

    /// <summary>فرمول استاندارد: ساعت اول + (ساعت‌های بعدی × هزینه ساعت اضافه).</summary>
    protected decimal CalculateStandardFee(int billableHours)
        => FirstHourFee + (billableHours - 1) * AdditionalHourFee;

    /// <summary>اعمال تخفیف VIP؛ فقط یک‌جا نوشته شده و در فرزندان تکرار نمی‌شود.</summary>
    protected decimal ApplyVipDiscount(decimal fee)
        => IsVip ? Math.Round(fee * (1 - VipDiscountRate), 0, MidpointRounding.AwayFromZero) : fee;

    /// <summary>توضیحات اختصاصی هر نوع برای نمایش (مثلاً ظرفیت کامیون).</summary>
    protected virtual string GetExtraInfo() => string.Empty;

    public static string NormalizePlate(string? plate)
        => (plate ?? string.Empty).Trim().ToUpperInvariant();

    public override string ToString()
    {
        string extra = GetExtraInfo();
        return $"{VehicleTypeName,-12} | {LicensePlate,-14} | {OwnerName,-15} | {EntryTime:yyyy-MM-dd HH:mm} | " +
               $"{(IsVip ? "VIP" : "-"),-3}{(extra.Length > 0 ? " | " + extra : string.Empty)}";
    }
}
