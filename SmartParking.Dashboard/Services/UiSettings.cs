using Microsoft.JSInterop;

namespace SmartParking.Dashboard.Services;

/// <summary>
/// تنظیمات ظاهری هر کاربر (تم، انیمیشن، تراکم جدول، تعداد ردیف هر صفحه). در localStorage مرورگر نگهداری می‌شود
/// و بر داده‌های پارکینگ هیچ اثری ندارد.
/// </summary>
public sealed class UiSettings
{
    public static readonly int[] PageSizes = { 10, 25, 50 };

    private readonly IJSRuntime _js;
    private bool _loaded;

    public UiSettings(IJSRuntime js)
    {
        _js = js;
    }

    public event Action? Changed;

    public string Theme { get; private set; } = "dark";
    public bool Animations { get; private set; } = true;
    public string Density { get; private set; } = "comfortable";
    public int PageSize { get; private set; } = 10;

    public async Task LoadAsync()
    {
        if (_loaded) return;
        try
        {
            var prefs = await _js.InvokeAsync<Dictionary<string, string>>("smartParking.getPrefs");
            if (prefs.TryGetValue("theme", out var theme) && theme is "dark" or "light") Theme = theme;
            if (prefs.TryGetValue("anim", out var anim)) Animations = anim != "off";
            if (prefs.TryGetValue("density", out var density) && density is "comfortable" or "compact") Density = density;
            if (prefs.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out int size) && PageSizes.Contains(size)) PageSize = size;
            _loaded = true;
            Changed?.Invoke();
        }
        catch (Exception ex) when (ex is JSException or InvalidOperationException or TaskCanceledException)
        {
            // در صورت قطع ارتباط مرورگر، مقادیر پیش‌فرض می‌مانند.
        }
    }

    public Task SetThemeAsync(string theme) => Set("theme", theme, () => Theme = theme);
    public Task SetAnimationsAsync(bool on) => Set("anim", on ? "on" : "off", () => Animations = on);
    public Task SetDensityAsync(string density) => Set("density", density, () => Density = density);
    public Task SetPageSizeAsync(int size) => Set("pageSize", size.ToString(), () => PageSize = size);

    private async Task Set(string key, string value, Action apply)
    {
        apply();
        try { await _js.InvokeVoidAsync("smartParking.setPref", key, value); }
        catch (Exception ex) when (ex is JSException or InvalidOperationException or TaskCanceledException) { }
        Changed?.Invoke();
    }
}
