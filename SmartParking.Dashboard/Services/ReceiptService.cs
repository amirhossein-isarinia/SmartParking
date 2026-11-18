using SmartParking.Services;

namespace SmartParking.Dashboard.Services;

/// <summary>نگهدارنده‌ی رسیدی که در پنجره‌ی رسید نمایش داده می‌شود (مخصوص هر نشست کاربر).</summary>
public sealed class ReceiptService
{
    public ExitRecord? Current { get; private set; }
    public int Number { get; private set; }

    public event Action? Changed;

    public void Show(ExitRecord record, int number)
    {
        Current = record;
        Number = number;
        Changed?.Invoke();
    }

    public void Close()
    {
        Current = null;
        Changed?.Invoke();
    }
}
