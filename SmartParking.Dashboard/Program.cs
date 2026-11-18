using System.Net;
using SmartParking.Dashboard.Components;
using SmartParking.Dashboard.Services;
using SmartParking.Services;

var builder = WebApplication.CreateBuilder(args);

// فایل‌های استاتیک بسته‌های NuGet (مثل blazor.web.js) در حالت Development خودکار فعال‌اند؛
// این خط باعث می‌شود با `dotnet run` در هر محیطی هم سرو شوند.
if (!builder.Environment.IsDevelopment())
    builder.WebHost.UseStaticWebAssets();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents(options => options.DetailedErrors = false);

// ---- منطق اصلی پروژه: یک نمونه‌ی مشترک و کنترل‌شده از ParkingManager برای تمام صفحات و تب‌ها ----
builder.Services.AddSingleton<ParkingManager>();
builder.Services.AddSingleton<ParkingStateService>();

// ---- سرویس‌های داشبورد (بدون منطق تجاری تکراری؛ فقط روی داده‌ی واقعی کار می‌کنند) ----
builder.Services.AddSingleton<TariffCatalog>();
builder.Services.AddSingleton<FeeBreakdownService>();
builder.Services.AddSingleton<DashboardService>();
builder.Services.AddSingleton<VehicleService>();
builder.Services.AddSingleton<FinancialService>();
builder.Services.AddSingleton<AnalyticsService>();

// ---- سرویس‌های مخصوص هر نشست کاربر ----
builder.Services.AddScoped<UiSettings>();
builder.Services.AddScoped<BrowserService>();
builder.Services.AddScoped<ReceiptService>();

var app = builder.Build();

// نسخه‌ی محلی: فقط درخواست‌های همین رایانه پذیرفته می‌شوند.
app.Use(async (context, next) =>
{
    IPAddress? remote = context.Connection.RemoteIpAddress;
    if (remote is null || !IPAddress.IsLoopback(remote))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
    await next();
});

app.UseExceptionHandler("/Error", createScopeForErrors: true);
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
