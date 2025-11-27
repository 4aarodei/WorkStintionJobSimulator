using WorkstationJobSimulator.Events;
using WorkstationJobSimulator.Models.wsModels;
using WorkstationJobSimulator.Physic.EventPhysic.Interface;

public class TurningOffTheLightsPhysics : IEventPhysics
{
    public Type EventType => typeof(TurningOffTheLights);

    private const double StandbyPowerWatts = 10.0;
    private const double AlarmPowerWatts = 150.0;
    private const double ChargePowerWatts = 30.0;

    private static readonly Random _random = new();

    public void Apply(Workstation ws, SimulationEvent ev)
    {
        var off = (TurningOffTheLights)ev;

        ws.Log("=== Фізика: відключення світла ===");
        ws.SetPower(false, "Подія: відключення світла");

        // 1) Розряд у режимі очікування
        ws.BatteryPhysics.ConsumeEnergy(
            ws,
            StandbyPowerWatts,
            off.Duration,
            $"Режим очікування при відключенні світла ({off.Duration.TotalHours:F1} год)");

        // 2) Саб-івенти тривоги
        foreach (var subAlarm in off.SubEvents.OfType<AirAlarm>())
        {
            ws.Log(">>> Під час відключення світла сталася повітряна тривога (SubEvent)!");

            var alarmDuration = subAlarm.Duration == TimeSpan.Zero
                ? TimeSpan.FromMinutes(2)
                : subAlarm.Duration;

            ws.SetAirAlarm(true, subAlarm.EventName ?? "Тривога під час відключення світла");

            ws.BatteryPhysics.ConsumeEnergy(
                ws,
                AlarmPowerWatts,
                alarmDuration,
                $"{subAlarm.EventName ?? "Повітряна тривога під час відключення світла"} ({alarmDuration.TotalMinutes} хв)");

            ws.SetAirAlarm(false, "Кінець тривоги під час відключення");
        }

        // 3) Світло повертається
        ws.SetPower(true, "Світло повернулося після відключення");

        // 4) Зарядка батареї
        var chargeDuration = RollRandomChargeDuration();

        ws.BatteryPhysics.ChargeBattery(
            ws,
            ChargePowerWatts,
            chargeDuration,
            $"Заряджання батареї після відновлення світла ({chargeDuration.TotalHours:F1} год)");

        ws.Log("=== Кінець фізики: відключення світла ===");
    }

    private static TimeSpan RollRandomChargeDuration() //(6 год) = 15%
    {
        int minHours = 1;
        int maxHours = 6;
        double probabilityMaxHours = 0.15;

        if (minHours >= maxHours)
            throw new ArgumentException("minHours має бути < maxHours");

        if (probabilityMaxHours <= 0 || probabilityMaxHours >= 1)
            throw new ArgumentOutOfRangeException(nameof(probabilityMaxHours),
                "Ймовірність має бути між 0 та 1 (невключно).");

        double continueProbability = Math.Pow(
            probabilityMaxHours,
            1.0 / (maxHours - minHours));

        int hours = minHours;

        while (hours < maxHours && _random.NextDouble() < continueProbability)
        {
            hours++;
        }

        return TimeSpan.FromHours(hours);
    }
}
