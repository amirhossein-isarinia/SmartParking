# نمودار کلاس‌ها (UML)

این نمودار با Mermaid نوشته شده است. می‌توانید آن را در https://mermaid.live یا در VS Code (افزونه Markdown Preview Mermaid) باز کنید، تصویرش را بگیرید و در PDF تحویلی قرار دهید.

```mermaid
classDiagram
    class Vehicle {
        <<abstract>>
        +const VipDiscountRate : decimal
        +LicensePlate : string
        +OwnerName : string
        +EntryTime : DateTime
        +IsVip : bool
        +VehicleTypeName : string*
        #FirstHourFee : decimal*
        #AdditionalHourFee : decimal*
        +CalculateParkingFee(exitTime) decimal*
        +GetParkingDuration(exitTime) TimeSpan
        #CalculateBillableHours(exitTime) int
        #CalculateStandardFee(hours) decimal
        #ApplyVipDiscount(fee) decimal
        #GetExtraInfo() string
        +NormalizePlate(plate)$ string
    }
    class Car { +DoorCount : int }
    class Motorcycle { +EngineCapacityCc : int }
    class Truck { +CapacityTons : decimal }
    class ElectricCar {
        +BatteryCapacityKwh : decimal
        +UsesParkingCharger : bool
    }
    class Bus { +SeatCount : int }
    class ExitRecord {
        +Vehicle : Vehicle
        +ExitTime : DateTime
        +Fee : decimal
        +Duration : TimeSpan
    }
    class ParkingManager {
        +const Capacity : int = 50
        -List~Vehicle~ parkedVehicles
        -List~ExitRecord~ exitHistory
        +RegisterEntry(vehicle)
        +RegisterExit(plate, exitTime) ExitRecord
        +FindByLicensePlate(plate) Vehicle
        +GetTotalRevenue() decimal
        +GetParkedCountByType() Dictionary
        +GetRevenueByDate(date) decimal
    }
    class ConsoleMenu { +Run() }

    Vehicle <|-- Car
    Vehicle <|-- Motorcycle
    Vehicle <|-- Truck
    Vehicle <|-- ElectricCar
    Vehicle <|-- Bus
    ParkingManager "1" o-- "0..50" Vehicle
    ParkingManager "1" o-- "*" ExitRecord
    ExitRecord --> Vehicle
    ConsoleMenu --> ParkingManager
```

## تصمیم طراحی درباره ElectricCar

ElectricCar مستقیماً از Vehicle ارث می‌برد، نه از Car. دلایل:

1. تعرفه‌ی آن متفاوت است (۲۰٬۰۰۰ / ۱۵٬۰۰۰) و اگر از Car ارث می‌برد باید همه‌ی خصوصیات تعرفه‌ای Car را بازنویسی می‌کرد.
2. ویژگی‌های Car (مثل تعداد درب) برای خودرو برقی در این مسئله معنایی ندارد.
3. رابطه‌ی ارث‌بری باید «is-a» واقعی و بدون تحمیل رفتار اضافی باشد (اصل جایگزینی لیسکوف).
