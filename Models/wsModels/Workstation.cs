using WorkstationJobSimulator.Events;
using WorkstationJobSimulator.Physic;

namespace WorkstationJobSimulator.Models.wsModels;

public class Workstation
{
    private readonly object _lock = new();

    public string Name { get; }

    public WorkstationState State { get; private set; } = WorkstationState.Idle;

    public PowerSystem PowerSystem { get; } = new();
    public Battery Battery { get; } = new();
    public BatteryPhysics BatteryPhysics { get; }

    public Amplifier Amplifier { get; } = new();
    public SystemStorage Storage { get; } = new();
    public NetworkInterface Network { get; } = new();

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

        // На старті симульований час ще не відомий,
        // тому перший лог піде з реальним часом (див. LogState).
        LogState("Ініціалізовано робочу станцію");
        PrintStatus();
    }

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
            // Якщо сим-час ще не заданий (default == 01.01.0001),
            // тоді показуємо реальний час, інакше — симульований.
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
            Console.WriteLine($"  Батарея:     {Battery.ChargePercent}% (Status: {Battery.Status})");
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
}
