using WorkstationJobSimulator.Models.wsModels;
using WorkstationJobSimulator.Physic;

namespace WorkstationJobSimulator.Events;

public abstract class SimulationEvent
{
    public string EventName { get; protected set; } = string.Empty;
    public TimeSpan Duration { get; protected set; } = TimeSpan.Zero;

    /// Додаткові підівенти (наприклад, повітряна тривога під час відключення світла).
    /// Їх обробляє відповідна фізика (наприклад, TurningOffTheLightsPhysics),
    /// тому тут це лише контейнер даних.
    public List<SimulationEvent> SubEvents { get; } = new();

    /// Виконує подію для вказаної робочої станції, делегуючи всю логіку
    /// у WorkstationPhysicsEngine та конкретні класи фізики (IEventPhysics).
    public void Execute(Workstation workstation, WorkstationPhysicsEngine physicsEngine)
    {
        if (workstation is null) throw new ArgumentNullException(nameof(workstation));
        if (physicsEngine is null) throw new ArgumentNullException(nameof(physicsEngine));

        physicsEngine.ApplyPhysics(workstation, this);
    }
}

[EventChance(1.4)] // ~1.4 повітряні тривоги на добу
public class AirAlarm : SimulationEvent
{
    public AirAlarm()
    {
        EventName = "Повітряна тривога";
        // Тривалість як "ціна" двох оголошень (старт/відбій)
        Duration = TimeSpan.FromMinutes(2);
    }
}

[EventChance(2.5)] // ~2.5 відключення світла на добу
public class TurningOffTheLights : SimulationEvent
{
    private static readonly Random _random = new();

    // ймовірність, що під час відключення буде ще й тривога
    private const double AirAlarmOverlapChance = 0.30; // ~30%

    public TurningOffTheLights()
    {
        EventName = "Відключення світла";
        Duration = RollDuration();

        if (_random.NextDouble() < AirAlarmOverlapChance)
        {
            SubEvents.Add(new AirAlarm());
        }
    }

    private static TimeSpan RollDuration()
    {
        // Типові "вікна" 3–5 год, іноді 6
        int[] hoursOptions = { 3, 3, 4, 4, 5, 5, 6 };
        int hours = hoursOptions[_random.Next(hoursOptions.Length)];
        return TimeSpan.FromHours(hours);
    }
}
