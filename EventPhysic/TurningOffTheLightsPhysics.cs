using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkstationJobSimulator.Events;
using WorkstationJobSimulator.Models.wsModels;

namespace WorkstationJobSimulator.PhysicsRegistration;

// TurningOffTheLightsPhysics.cs
public class TurningOffTheLightsPhysics : IEventPhysics
{
    public Type EventType => typeof(TurningOffTheLights);

    public void Apply(Workstation ws, SimulationEvent ev)
    {
        if (ev is not TurningOffTheLights off)
            throw new ArgumentException("TurningOffTheLightsPhysics може обробляти тільки TurningOffTheLights", nameof(ev));

        ws.Log("=== Фізика: відключення світла ===");
        ws.SetPower(false, "Подія: відключення світла");

        // Споживання в режимі "просто сидимо без світла" – мінімальний режим
        const double standbyPowerWatts = 40.0;

        ws.BatteryPhysics.ConsumeEnergy(
            ws,
            standbyPowerWatts,
            off.Duration,
            $"Режим очікування при відключенні світла ({off.Duration.TotalHours:F1} год)");

        // Якщо під час відключення був підівент AirAlarm – окремо добиваємо батарею
        foreach (var sub in off.SubEvents)
        {
            if (sub is AirAlarm air)
            {
                ws.Log(">>> Під час відключення світла сталася повітряна тривога (SubEvent)!");

                ws.SetAirAlarm(true, "Тривога під час відключення світла");

                const double alarmPowerWatts = 150.0;
                ws.BatteryPhysics.ConsumeEnergy(
                    ws,
                    alarmPowerWatts,
                    air.Duration,
                    $"Повітряна тривога під час відключення світла ({air.Duration.TotalMinutes:F0} хв)");

                ws.SetAirAlarm(false, "Кінець тривоги під час відключення");
            }
        }

        ws.SetPower(true, "Світло повернулося після відключення");
        ws.Log("=== Кінець фізики: відключення світла ===");
    }

}