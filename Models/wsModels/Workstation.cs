using System;
using System.Linq;
using WorkstationJobSimulator.Events;
using WorkstationJobSimulator.Models;
using WorkstationJobSimulator.Physic;

namespace WorkstationJobSimulator.Models.wsModels;

public class Workstation
{
    private readonly object _lock = new();

    public string Name { get; }

    public WorkstationState State { get; private set; } = WorkstationState.Idle;

    public double AmbientTemp { get; set; } = 20.0;


    public PowerSystem PowerSystem { get; } = new();
    public Battery Battery { get; } = new();
    public BatteryPhysics BatteryPhysics { get; }

    public Amplifier Amplifier { get; } = new();
    public SystemStorage Storage { get; } = new();
    public NetworkInterface Network { get; } = new();

    /// <summary>Стан мікроконтролера.</summary>
    public McuState McuState { get; set; } = McuState.Ok;

    /// <summary>Стан вхідного сигналу.</summary>
    public SignalState SignalState { get; set; } = SignalState.Ok;

    /// <summary>Ручні елементи, що впливають на потужність підсилювача.</summary>
    public List<ManualElement> ManualElements { get; } = new();

    /// <summary>Номінальна вихідна потужність підсилювача (Вт).</summary>
    public double MaxOutputPowerWatts { get; set; } = 600.0;

    /// <summary>Чи є зараз живлення на станції.</summary>
    public bool IsPowerOn { get; private set; } = true;

    /// <summary>Чи активна зараз повітряна тривога.</summary>
    public bool IsAirAlarmActive { get; private set; } = false;

    /// <summary>
    /// Поточний симульований час для цієї станції.
    /// Встановлюється ззовні (наприклад, у Program перед кожною "годиною").
    /// </summary>
    public DateTime SimulationTime { get; private set; }

    public Workstation(string name)
    {
        Name = name;
        BatteryPhysics = new BatteryPhysics();

        // 8 ручних елементів за замовчуванням (можна винести в конфіг)
        const int defaultManualElementsCount = 8;
        for (int i = 0; i < defaultManualElementsCount; i++)
        {
            ManualElements.Add(new ManualElement());
        }

        // На старті симульований час ще не відомий,
        // тому перший лог піде з реальним часом (див. LogState).
        LogState("Ініціалізовано робочу станцію");
        PrintStatus();
    }

    /// <summary>
    /// Кількість ручних елементів.
    /// </summary>
    public int TotalManualElements => ManualElements.Count;

    /// <summary>
    /// Кількість працездатних ручних елементів.
    /// </summary>
    public int WorkingManualElements => ManualElements.Count(e => e.State == ManualElementState.Ok);

    /// <summary>
    /// Фактор ручних елементів (0..1), що множиться на потужність підсилювача.
    /// </summary>
    public double ManualElementsFactor =>
        TotalManualElements == 0 ? 1.0 : (double)WorkingManualElements / TotalManualElements;

    /// <summary>
    /// Оновити симульований час станції.
    /// Викликай це в Program перед генерацією події на поточну годину.
    /// </summary>
    public void SetSimulationTime(DateTime simTime)
    {
        lock (_lock)
        {
            SimulationTime = simTime;
        }
    }

    // =====================
    // Базова утиліта стану
    // =====================

    public void LogState(string message)
    {
        lock (_lock)
        {
            var time = SimulationTime == default
                ? DateTime.Now
                : SimulationTime;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"[{time:HH:mm:ss}] [WS:{Name}] ");
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }

    public void Log(string message)
    {
        lock (_lock)
        {
            Console.WriteLine($"    {message}");
        }
    }

    /// <summary>
    /// Вивести поточний стан станції в консоль.
    /// </summary>
    public void PrintStatus()
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  --- Поточний стан робочої станції ---");
            Console.ResetColor();
            Console.WriteLine($"  Стан:        {State}");
            Console.WriteLine($"  Світло:      {(IsPowerOn ? "УВІМКНЕНО" : "ВИМКНЕНО")}");
            Console.WriteLine($"  Тривога:     {(IsAirAlarmActive ? "АКТИВНА" : "НЕМАЄ")}");
            Console.WriteLine(
                $"  Батарея:     {Battery.ChargePercent}% SoH={Battery.HealthPercent:F1}% " +
                $"(Status: {Battery.Status}, HealthState: {Battery.HealthState})");
            Console.WriteLine(
                $"  Ручні ел.:   {WorkingManualElements}/{TotalManualElements} " +
                $"(factor={ManualElementsFactor:F2})");
            Console.WriteLine($"  Потужність:  {GetCurrentOutputPower():F1} Вт");
            Console.WriteLine("  -------------------------------------");
        }
    }

    // =====================
    // Методи, які керують станом
    // =====================

    public void SetPower(bool isOn, string reason)
    {
        IsPowerOn = isOn;
        LogState($"Світло {(IsPowerOn ? "УВІМКНЕНО" : "ВИМКНЕНО")} ({reason})");
        PrintStatus();
    }

    public void SetAirAlarm(bool isActive, string reason)
    {
        IsAirAlarmActive = isActive;
        LogState($"Повітряна тривога {(IsAirAlarmActive ? "АКТИВНА" : "НЕМАЄ")} ({reason})");
        PrintStatus();
    }

    /// <summary>
    /// Узагальнений стан живлення (AC/DC/NoPower).
    /// </summary>
    /// <summary>
    /// Узагальнений стан живлення (AC/DC/NoPower).
    /// </summary>
    public PowerState GetPowerState()
    {
        // Якщо станцію формально вимкнули – вважаємо, що живлення немає
        if (!IsPowerOn || PowerSystem.Mode == PowerMode.Off)
            return PowerState.NoPower;

        // 1) Перевіряємо мережу 220В
        // Є 220В + запобіжник цілий → живлення від мережі (AC)
        bool hasGrid = PowerSystem.MainVoltage > 180.0 && PowerSystem.IsFuseOk;
        if (hasGrid)
            return PowerState.Ac;

        // 2) Якщо мережі немає, пробуємо перейти на батарею
        bool batteryAlive =
            Battery.Status == BatteryStatus.Ok &&                  // не Cutoff
            Battery.HealthState != BatteryHealthState.Fail &&      // SoH не Fail
            Battery.ChargePercent > 0;                             // ще є заряд

        if (batteryAlive)
            return PowerState.Dc;

        // 3) Немає ні мережі, ні живої батареї
        return PowerState.NoPower;
    }

    /// <summary>
    /// Поточна доступна вихідна потужність підсилювача з урахуванням станів компонентів.
    /// </summary>
    public double GetCurrentOutputPower()
    {
        // 1. Немає сигналу – немає виходу
        if (SignalState == SignalState.Fail)
            return 0.0;

        // 2. MCU не працює – логіка керування відсутня
        if (McuState == McuState.Fail)
            return 0.0;

        // 3. Живлення
        var powerState = GetPowerState();
        if (powerState == PowerState.NoPower)
            return 0.0;

        // Якщо живлення від батареї, але вона в Cutoff або HealthState.Fail – 0
        if (powerState == PowerState.Dc &&
            (Battery.Status == BatteryStatus.Cutoff || Battery.HealthState == BatteryHealthState.Fail))
        {
            return 0.0;
        }

        // 4. Фактор ручних елементів
        var manualFactor = ManualElementsFactor;

        // 5. Фактор батареї (якщо деградована – нижча потужність)
        double batteryFactor = 1.0;
        if (Battery.HealthState == BatteryHealthState.Degraded)
            batteryFactor = Battery.HealthPercent / 100.0;
        else if (Battery.HealthState == BatteryHealthState.Fail)
            batteryFactor = 0.0;

        // 6. Чи активна зараз тривога
        bool alarmActive = IsAirAlarmActive;

        // Невелика standby-потужність, коли станція просто "чекає"
        const double standbyPowerWatts = 30.0;

        // При тривозі – повна потужність, без тривоги – standby
        double basePowerWatts = alarmActive
            ? MaxOutputPowerWatts
            : standbyPowerWatts;

        return basePowerWatts * manualFactor * batteryFactor;
    }

}
