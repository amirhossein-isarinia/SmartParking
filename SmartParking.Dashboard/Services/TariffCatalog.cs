using SmartParking.Models;

namespace SmartParking.Dashboard.Services;

public sealed record TariffInfo(
    string Key,
    decimal FirstHour,
    decimal AdditionalHour,
    decimal ChargerFee,
    string? DiscountLabel,
    string? SurchargeLabel,
    IReadOnlyList<string> Notes);

/// <summary>
/// تعرفه‌ها را مستقیماً از رفتار واقعی کلاس‌های اصلی استخراج می‌کند (با اجرای <c>CalculateParkingFee</c> روی
/// وسایل آزمایشی). بنابراین اگر تعرفه در پروژه اصلی تغییر کند، داشبورد خودکار هماهنگ می‌ماند
/// و هیچ عدد تعرفه‌ای در داشبورد کدنویسی نشده است.
/// </summary>
public sealed class TariffCatalog
{
    private static readonly DateTime T0 = new(2000, 1, 1, 8, 0, 0);
    private readonly Dictionary<string, TariffInfo> _items = new();

    public TariffCatalog()
    {
        foreach (var type in VehicleTypes.All)
            _items[type.Key] = Probe(type.Key);
    }

    public decimal VipDiscountRate => Vehicle.VipDiscountRate;

    public IReadOnlyList<TariffInfo> All => VehicleTypes.All.Select(t => _items[t.Key]).ToList();

    public TariffInfo Get(string key) => _items[key];

    private static Vehicle MakeProbe(string key, decimal tons = 1m, bool charger = false)
        => VehicleFactory.Create(key, "PROBE-1", "probe", T0, false, new VehicleSpecs(Tons: tons, Charger: charger));

    private static decimal FeeAt(Vehicle v, int hours) => v.CalculateParkingFee(T0.AddHours(hours));

    private static TariffInfo Probe(string key)
    {
        Vehicle baseProbe = MakeProbe(key);
        decimal first = FeeAt(baseProbe, 1);
        decimal additional = FeeAt(baseProbe, 2) - first;
        decimal charger = 0;
        string? discountLabel = null, surchargeLabel = null;
        var notes = new List<string>();

        decimal Standard(int h) => first + (h - 1) * additional;

        if (key == "Motorcycle")
        {
            for (int h = 2; h <= 72; h++)
            {
                decimal actual = FeeAt(baseProbe, h);
                if (actual != Standard(h))
                {
                    int threshold = h - 1;
                    decimal rate = Math.Round(1 - actual / Standard(h), 4);
                    discountLabel = $"تخفیف توقف طولانی (بیش از {Fa.Number(threshold)} ساعت — {Fa.Percent((double)rate * 100)})";
                    notes.Add($"توقف بیش از {Fa.Number(threshold)} ساعت: {Fa.Percent((double)rate * 100)} تخفیف");
                    break;
                }
            }
        }
        else if (key == "Truck")
        {
            decimal? heavyAt = null;
            for (decimal cap = 1m; cap <= 100m; cap += 0.5m)
            {
                if (FeeAt(MakeProbe(key, cap), 1) != first) { heavyAt = cap; break; }
            }

            if (heavyAt is decimal hi)
            {
                decimal lo = hi - 0.5m;
                decimal threshold = lo;
                for (decimal c = lo + 0.01m; c <= hi; c += 0.01m)
                {
                    if (FeeAt(MakeProbe(key, c), 1) != first) { threshold = c - 0.01m; break; }
                }

                decimal rate = Math.Round(FeeAt(MakeProbe(key, hi), 1) / first - 1, 4);
                surchargeLabel = $"هزینه اضافه بار سنگین (بیش از {Fa.Decimal(threshold)} تن — {Fa.Percent((double)rate * 100)})";
                notes.Add($"ظرفیت بار بیش از {Fa.Decimal(threshold)} تن: {Fa.Percent((double)rate * 100)} هزینه اضافه");
            }
        }
        else if (key == "ElectricCar")
        {
            charger = FeeAt(MakeProbe(key, charger: true), 1) - FeeAt(MakeProbe(key, charger: false), 1);
            if (charger != 0)
                notes.Add($"هزینه شارژ: {Fa.Money(charger)} در صورت استفاده از شارژر");
        }

        return new TariffInfo(key, first, additional, charger, discountLabel, surchargeLabel, notes);
    }
}
