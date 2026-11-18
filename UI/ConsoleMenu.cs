using SmartParking.Models;
using SmartParking.Services;

namespace SmartParking.UI;

/// <summary>
/// رابط کنسول. فقط ورودی/خروجی را مدیریت می‌کند و هیچ فرمول هزینه‌ای در آن وجود ندارد.
/// </summary>
public class ConsoleMenu
{
    private readonly ParkingManager _manager;

    public ConsoleMenu(ParkingManager manager)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    public void Run()
    {
        bool running = true;
        while (running)
        {
            ShowMenu();
            int choice = InputHelper.ReadInt("Select an option: ", 0, 7);
            Console.WriteLine();

            try
            {
                switch (choice) // این switch روی گزینه منو است، نه روی نوع شیء
                {
                    case 1: RegisterEntry(); break;
                    case 2: RegisterExit(); break;
                    case 3: ShowParkedVehicles(); break;
                    case 4: SearchByPlate(); break;
                    case 5: ShowRevenue(); break;
                    case 6: ShowStatistics(); break;
                    case 7: ShowDailyRevenue(); break;
                    case 0: running = false; break;
                }
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                Console.WriteLine($"  Error: {ex.Message}");
            }

            if (running)
            {
                Console.WriteLine();
                Console.Write("Press Enter to continue...");
                Console.ReadLine();
            }
        }

        Console.WriteLine("Goodbye!");
    }

    private static void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("SMART PARKING MANAGEMENT");
        Console.WriteLine("---------------------------------");
        Console.WriteLine("1. Register Vehicle Entry");
        Console.WriteLine("2. Register Vehicle Exit");
        Console.WriteLine("3. Show Parked Vehicles");
        Console.WriteLine("4. Search by License Plate");
        Console.WriteLine("5. Show Parking Revenue");
        Console.WriteLine("6. Show Vehicle Statistics");
        Console.WriteLine("7. Daily Revenue Report (Challenge)");
        Console.WriteLine("0. Exit");
    }

    // ---------- 1. ورود ----------
    private void RegisterEntry()
    {
        if (_manager.IsFull)
        {
            Console.WriteLine($"  Parking is full ({ParkingManager.Capacity}/{ParkingManager.Capacity}). Entry is not allowed.");
            return;
        }

        Console.WriteLine("Vehicle types: 1) Car  2) Motorcycle  3) Truck  4) Electric Car  5) Bus");
        int type = InputHelper.ReadInt("Select vehicle type: ", 1, 5);

        string plate = InputHelper.ReadNonEmpty("License plate: ");
        if (_manager.FindByLicensePlate(plate) is not null)
        {
            Console.WriteLine("  This license plate is already registered in the parking.");
            return;
        }

        string owner = InputHelper.ReadNonEmpty("Owner name: ");
        bool isVip = InputHelper.ReadYesNo("VIP subscription?");

        DateTime entryTime = InputHelper.ReadOptionalDateTime("Entry time");
        if (entryTime > DateTime.Now)
        {
            Console.WriteLine("  Entry time cannot be in the future.");
            return;
        }

        Vehicle vehicle = CreateVehicle(type, plate, owner, entryTime, isVip);
        _manager.RegisterEntry(vehicle);

        Console.WriteLine();
        Console.WriteLine($"  Entry registered: {vehicle.VehicleTypeName} [{vehicle.LicensePlate}] at {entryTime.ToString(InputHelper.DateTimeFormat)}");
        Console.WriteLine($"  Occupancy: {_manager.ParkedCount}/{ParkingManager.Capacity}");
    }

    /// <summary>ساخت شیء بر اساس گزینه انتخابی کاربر (نه بررسی نوع شیء).</summary>
    private static Vehicle CreateVehicle(int type, string plate, string owner, DateTime entryTime, bool isVip)
    {
        switch (type)
        {
            case 1:
                int doors = InputHelper.ReadInt("Number of doors (2-5): ", 2, 5);
                return new Car(plate, owner, entryTime, doors, isVip);
            case 2:
                int cc = InputHelper.ReadInt("Engine capacity in cc (50-2000): ", 50, 2000);
                return new Motorcycle(plate, owner, entryTime, cc, isVip);
            case 3:
                decimal tons = InputHelper.ReadDecimal("Load capacity in tons (0-100]: ", 0m, 100m);
                return new Truck(plate, owner, entryTime, tons, isVip);
            case 4:
                decimal kwh = InputHelper.ReadDecimal("Battery capacity in kWh (0-300]: ", 0m, 300m);
                bool charger = InputHelper.ReadYesNo("Uses parking charger?");
                return new ElectricCar(plate, owner, entryTime, kwh, charger, isVip);
            default:
                int seats = InputHelper.ReadInt("Seat count (10-100): ", 10, 100);
                return new Bus(plate, owner, entryTime, seats, isVip);
        }
    }

    // ---------- 2. خروج ----------
    private void RegisterExit()
    {
        if (_manager.ParkedCount == 0)
        {
            Console.WriteLine("  The parking is empty.");
            return;
        }

        string plate = InputHelper.ReadNonEmpty("License plate: ");
        Vehicle? vehicle = _manager.FindByLicensePlate(plate);
        if (vehicle is null)
        {
            Console.WriteLine("  No parked vehicle found with this license plate.");
            return;
        }

        DateTime exitTime = InputHelper.ReadOptionalDateTime("Exit time");
        ExitRecord record = _manager.RegisterExit(plate, exitTime);

        Console.WriteLine();
        Console.WriteLine("  ----- Exit Receipt -----");
        Console.WriteLine($"  Vehicle   : {record.Vehicle.VehicleTypeName} [{record.Vehicle.LicensePlate}]");
        Console.WriteLine($"  Owner     : {record.Vehicle.OwnerName}{(record.Vehicle.IsVip ? " (VIP)" : string.Empty)}");
        Console.WriteLine($"  Entry     : {record.Vehicle.EntryTime.ToString(InputHelper.DateTimeFormat)}");
        Console.WriteLine($"  Exit      : {record.ExitTime.ToString(InputHelper.DateTimeFormat)}");
        Console.WriteLine($"  Duration  : {InputHelper.FormatDuration(record.Duration)}");
        Console.WriteLine($"  Final Fee : {InputHelper.FormatMoney(record.Fee)}");
    }

    // ---------- 3. نمایش وسایل حاضر ----------
    private void ShowParkedVehicles()
    {
        if (_manager.ParkedCount == 0)
        {
            Console.WriteLine("  No vehicles are currently parked.");
            return;
        }

        Console.WriteLine($"Parked vehicles ({_manager.ParkedCount}/{ParkingManager.Capacity}):");
        Console.WriteLine(new string('-', 100));
        int index = 1;
        foreach (Vehicle vehicle in _manager.ParkedVehicles)
            Console.WriteLine($"{index++,3}. {vehicle}");
    }

    // ---------- 4. جست‌وجو ----------
    private void SearchByPlate()
    {
        string plate = InputHelper.ReadNonEmpty("License plate: ");
        Vehicle? vehicle = _manager.FindByLicensePlate(plate);
        Console.WriteLine(vehicle is null
            ? "  No parked vehicle found with this license plate."
            : $"  Found: {vehicle}");
    }

    // ---------- 5. درآمد ----------
    private void ShowRevenue()
    {
        Console.WriteLine($"Total vehicles exited : {_manager.ExitHistory.Count}");
        Console.WriteLine($"Total parking revenue : {InputHelper.FormatMoney(_manager.GetTotalRevenue())}");
    }

    // ---------- 6. آمار ----------
    private void ShowStatistics()
    {
        Console.WriteLine($"Currently parked: {_manager.ParkedCount}/{ParkingManager.Capacity}");
        PrintCounts(_manager.GetParkedCountByType());

        Console.WriteLine();
        Console.WriteLine($"Total exited: {_manager.ExitHistory.Count}");
        PrintCounts(_manager.GetExitedCountByType());
    }

    private static void PrintCounts(IReadOnlyDictionary<string, int> counts)
    {
        if (counts.Count == 0)
        {
            Console.WriteLine("  (none)");
            return;
        }

        foreach (var pair in counts)
            Console.WriteLine($"  {pair.Key,-12}: {pair.Value}");
    }

    // ---------- 7. گزارش تاریخی (چالش سوم) ----------
    private void ShowDailyRevenue()
    {
        DateTime date = InputHelper.ReadDate("Report date");
        IReadOnlyList<ExitRecord> records = _manager.GetExitRecordsByDate(date);

        Console.WriteLine();
        Console.WriteLine($"Revenue report for {date.ToString(InputHelper.DateFormat)}:");
        if (records.Count == 0)
        {
            Console.WriteLine("  No exits were recorded on this date.");
            return;
        }

        foreach (ExitRecord r in records)
            Console.WriteLine($"  {r.ExitTime:HH:mm} | {r.Vehicle.VehicleTypeName,-12} | {r.Vehicle.LicensePlate,-14} | {InputHelper.FormatMoney(r.Fee)}");

        Console.WriteLine($"  Vehicles: {records.Count}");
        Console.WriteLine($"  Total   : {InputHelper.FormatMoney(_manager.GetRevenueByDate(date))}");
    }
}
