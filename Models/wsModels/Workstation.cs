using WorkstationJobSimulator.Events;
using WorkstationJobSimulator.Physics;

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

    public Workstation(string name)
    {
        Name = name;
        BatteryPhysics = new BatteryPhysics();
        LogState("Ініціалізовано робочу станцію");
        PrintStatus();
    }

    // =====================
    // Базова утиліта стану
    // =====================

    private void ChangeState(WorkstationState newState, string reason)
    {
        if (State == newState) return;

        State = newState;
        LogState($"Стан змінено на: {State} ({reason})");
    }

    public void LogState(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] [WS:{Name}] ");
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
