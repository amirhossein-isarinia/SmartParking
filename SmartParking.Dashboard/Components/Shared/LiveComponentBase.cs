using Microsoft.AspNetCore.Components;
using SmartParking.Dashboard.Services;

namespace SmartParking.Dashboard.Components.Shared;

/// <summary>
/// پایه‌ی صفحاتی که باید با تغییر داده‌ی پارکینگ (در همین تب یا تب‌های دیگر) خودکار به‌روز شوند.
/// کامپوننت‌ها در <see cref="OnDataChanged"/> داده‌ی نمایشی را از سرویس‌ها دوباره می‌خوانند؛
/// بازه‌ی زمانی اختیاری هم برای تازه‌سازی مدت توقف‌ها استفاده می‌شود. هیچ Refresh کامل صفحه‌ای لازم نیست.
/// </summary>
public abstract class LiveComponentBase : ComponentBase, IDisposable
{
    private Timer? _timer;
    private bool _disposed;

    [Inject] protected ParkingStateService State { get; set; } = default!;

    protected virtual TimeSpan? RefreshInterval => TimeSpan.FromSeconds(30);

    protected virtual void OnDataChanged() { }

    protected override void OnInitialized()
    {
        State.Changed += HandleStateChanged;
        if (RefreshInterval is TimeSpan interval)
            _timer = new Timer(_ => HandleStateChanged(), null, interval, interval);
        OnDataChanged();
    }

    private void HandleStateChanged()
    {
        if (_disposed) return;
        _ = InvokeAsync(() =>
        {
            if (_disposed) return;
            OnDataChanged();
            StateHasChanged();
        });
    }

    public virtual void Dispose()
    {
        _disposed = true;
        State.Changed -= HandleStateChanged;
        _timer?.Dispose();
    }
}
