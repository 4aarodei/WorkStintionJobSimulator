using WorkstationJobSimulator.Models.wsModels;
using WorkstationJobSimulator.PhysicsRegistration;

namespace WorkstationJobSimulator.Events;

public abstract class SimulationEvent
{
    public string EventName { get; protected set; } = string.Empty;
    public TimeSpan Duration { get; protected set; } = TimeSpan.Zero;

    /// <summary>
    /// Додаткові підівенти (наприклад, повітряна тривога під час відключення світла).
    /// Їх обробляє відповідна фізика (наприклад, TurningOffTheLightsPhysics),
    /// тому тут це лише контейнер даних.
    /// </summary>
    public List<SimulationEvent> SubEvents { get; } = new();

    /// <summary>
    /// Виконує подію для вказаної робочої станції, делегуючи всю логіку
    /// у WorkstationPhysicsEngine та конкретні класи фізики (IEventPhysics).
    /// </summary>
    public void Execute(Workstation workstation, WorkstationPhysicsEngine physicsEngine)
    {
        if (workstation is null) throw new ArgumentNullException(nameof(workstation));
        if (physicsEngine is null) throw new ArgumentNullException(nameof(physicsEngine));

        physicsEngine.ApplyPhysics(workstation, this);
    }
}

[EventChance(0.5)]
public class AirAlarm : SimulationEvent
{
    public AirAlarm()
    {
        EventName = "Повітряна тривога";
        Duration = TimeSpan.FromMinutes(2);
    }
}

[EventChance(0.35)]
public class TurningOffTheLights : SimulationEvent
{
    private static readonly Random _random = new();
    private const double AirAlarmOverlapChance = 0.15; // 15% шанс на тривогу під час відключення

    public TurningOffTheLights()
    {
        EventName = "Відключення світла";
        Duration = RollDuration();

        if (_random.NextDouble() < AirAlarmOverlapChance)
        {
            // під час відключення світла додатково трапляється повітряна тривога
            SubEvents.Add(new AirAlarm());
        }
    }

    private static TimeSpan RollDuration()
    {
        int hours = _random.Next(3, 8);
        return TimeSpan.FromHours(hours);
    }
}
