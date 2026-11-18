using SmartParking.Services;

namespace SmartParking.Dashboard.Services;

public sealed record UiError(string? Field, string Message);

/// <summary>
/// ترجمه‌ی خطاهای پروژه اصلی (که انگلیسی هستند) به پیام فارسی برای کاربر.
/// جزئیات داخلی Exception هرگز به کاربر نمایش داده نمی‌شود.
/// </summary>
public static class ErrorTranslator
{
    public static UiError Translate(Exception ex)
    {
        string m = ex.Message ?? string.Empty;

        if (m.StartsWith("License plate must be", StringComparison.Ordinal))
            return new("plate", "پلاک باید بین ۳ تا ۲۰ نویسه باشد.");
        if (m.StartsWith("License plate may only", StringComparison.Ordinal))
            return new("plate", "پلاک فقط می‌تواند شامل حروف، ارقام، خط تیره و فاصله باشد.");
        if (m.StartsWith("Owner name", StringComparison.Ordinal))
            return new("owner", "نام مالک نمی‌تواند خالی باشد.");
        if (m.StartsWith("Door count", StringComparison.Ordinal))
            return new("doors", "تعداد درب باید عددی صحیح بین ۲ تا ۵ باشد.");
        if (m.StartsWith("Engine capacity", StringComparison.Ordinal))
            return new("cc", "حجم موتور باید عددی صحیح بین ۵۰ تا ۲۰۰۰ سی‌سی باشد.");
        if (m.StartsWith("Truck capacity", StringComparison.Ordinal))
            return new("tons", "ظرفیت بار باید بیشتر از صفر و حداکثر ۱۰۰ تن باشد.");
        if (m.StartsWith("Battery capacity", StringComparison.Ordinal))
            return new("kwh", "ظرفیت باتری باید بیشتر از صفر و حداکثر ۳۰۰ کیلووات‌ساعت باشد.");
        if (m.StartsWith("Seat count", StringComparison.Ordinal))
            return new("seats", "تعداد صندلی باید عددی صحیح بین ۱۰ تا ۱۰۰ باشد.");
        if (m.StartsWith("Parking is full", StringComparison.Ordinal))
            return new(null, $"ظرفیت پارکینگ تکمیل است ({Fa.Number(ParkingManager.Capacity)} از {Fa.Number(ParkingManager.Capacity)}). ثبت ورود جدید ممکن نیست.");
        if (m.StartsWith("A vehicle with plate", StringComparison.Ordinal))
            return new("plate", "وسیله‌ای با این پلاک هم‌اکنون در پارکینگ حضور دارد و ثبت ورود تکراری مجاز نیست.");
        if (m.StartsWith("No parked vehicle found", StringComparison.Ordinal))
            return new("plate", "وسیله‌ای با این پلاک در پارکینگ یافت نشد.");
        if (m.StartsWith("Exit time cannot be earlier", StringComparison.Ordinal))
            return new("time", "زمان خروج نمی‌تواند قبل از زمان ورود باشد.");

        return new(null, "عملیات انجام نشد. لطفاً اطلاعات واردشده را بررسی کنید و دوباره تلاش کنید.");
    }
}
