using Microsoft.JSInterop;

namespace SmartParking.Dashboard.Services;

/// <summary>پل ارتباط با کارهایی که فقط مرورگر انجام می‌دهد: چاپ، دانلود فایل، فوکوس.</summary>
public sealed class BrowserService
{
    private readonly IJSRuntime _js;

    public BrowserService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>چاپ صفحه. در حالت «receipt» فقط رسید چاپ می‌شود.</summary>
    public async Task PrintAsync(string mode = "page")
    {
        try { await _js.InvokeVoidAsync("smartParking.print", mode); }
        catch (Exception ex) when (ex is JSException or InvalidOperationException or TaskCanceledException) { }
    }

    public async Task DownloadTextAsync(string fileName, string content, string mime = "text/csv;charset=utf-8")
    {
        try { await _js.InvokeVoidAsync("smartParking.download", fileName, content, mime); }
        catch (Exception ex) when (ex is JSException or InvalidOperationException or TaskCanceledException) { }
    }

    public async Task FocusAsync(string elementId)
    {
        try { await _js.InvokeVoidAsync("smartParking.focus", elementId); }
        catch (Exception ex) when (ex is JSException or InvalidOperationException or TaskCanceledException) { }
    }
}
