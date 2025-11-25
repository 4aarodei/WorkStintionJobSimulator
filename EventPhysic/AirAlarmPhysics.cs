using System;
using WorkstationJobSimulator.Events;
using WorkstationJobSimulator.Models.wsModels;

namespace WorkstationJobSimulator.PhysicsRegistration;

public class AirAlarmPhysics : IEventPhysics
{
    public Type EventType => typeof(AirAlarm);

    public void Apply(Workstation ws, SimulationEvent ev)
    {
        if (ev is not AirAlarm air)
            throw new ArgumentException(
                "AirAlarmPhysics може обробляти тільки події AirAlarm",
                nameof(ev));

        ws.Log("=== Фізика: повітряна тривога ===");
        ws.SetAirAlarm(true, "Подія: повітряна тривога");

        // Умовна потужність системи під час тривоги (Вт):
        const double alarmPower = 150.0;

        // Витрачаємо заряд батареї
        ws.BatteryPhysics.ConsumeEnergy(
            ws,
            alarmPower,
            air.Duration,
            $"Повітряна тривога ({air.Duration.TotalMinutes:F0} хв)");

        ws.SetAirAlarm(false, "Кінець повітряної тривоги");
        ws.Log("=== Кінець фізики: повітряна тривога ===");
    }
}
