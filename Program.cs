using System.Text;
using WorkstationJobSimulator.Events;
using WorkstationJobSimulator.Logers;
using WorkstationJobSimulator.Models;
using WorkstationJobSimulator.Models.wsModels;
using WorkstationJobSimulator.Physic;

namespace WorkstationJobSimulator;

public static class Program
{
    /// <summary>
    /// Скільки часу симулюємо (в годинах).
    ///  - 24  = 1 доба
    ///  - 240 = 10 діб
    /// </summary>
    private const int SimulationHours = 5000;

    /// <summary>
    /// Скільки реальних секунд триває 1 симульована година.
    /// </summary>
    private const double RealSecondsPerSimHour = 0.0000001;

    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Налаштовуємо дублювання консольного виводу у файл
        var logFilePath = SimulationLogger.Setup();
        Console.WriteLine($"[SYSTEM] Лог симуляції зберігатиметься у файлі: {logFilePath}");

        // Налаштовуємо файл для снапшотів стану
        var snapshotsFilePath = SnapshotLogger.Setup();
        Console.WriteLine($"[SYSTEM] Снапшоти стану будуть зберігатись у файлі: {snapshotsFilePath}");
        Console.WriteLine();

        var workstation = new Workstation("WS-01");
        var generator = new SimulationEventGenerator();

        var physicsEngine = new WorkstationPhysicsEngine();
        RegistrationPhysicsMaster.RegisterAllEventPhysics(physicsEngine);

        // Стартовий симульований час (умовна дата/година)
        var simStart = new DateTime(2025, 1, 1, 0, 0, 0);

        WriteSeparator("Починаємо симуляцію робочої станції (дискретно по годинах)...");

        for (int hour = 0; hour < SimulationHours; hour++)
        {
            WriteIterationHeader(hour + 1);

            // 1) Поточний симульований час
            var currentSimTime = simStart.AddHours(hour);

            // 2) Передаємо симульований час у станцію
            workstation.SetSimulationTime(currentSimTime);

            // 3) 1 тік = 1 година симуляції: генеруємо подію на цю годину
            var ev = generator.Generate();

            Console.WriteLine(
                $"[SIM] Поточна сим-година: {generator.CurrentSimHour} " +
                $"(сим-час: {currentSimTime:dd.MM HH:mm})");
            if (ev is null)
            {
                Console.WriteLine("[EVENT] У цю годину подій немає.");
            }
            else
            {
                Console.WriteLine($"[EVENT] {ev.EventName}, тривалість {ev.Duration}");

                // 4) Виконуємо подію
                ev.Execute(workstation, physicsEngine);

                Console.WriteLine();
                Console.WriteLine("[STATE] Стан станції після обробки події:");
                workstation.PrintStatus();
            }

            // 4.1) Фонове споживання при роботі від батареї без подій
            if (ev is null && workstation.GetPowerState() == PowerState.Dc)
            {
                // 5–10 Вт на годину — умовний Idle-режим від батареї
                const double idlePowerWatts = 8.0;

                workstation.BatteryPhysics.ConsumeEnergy(
                    workstation,
                    idlePowerWatts,
                    TimeSpan.FromHours(1),
                    "Фонове споживання в режимі очікування від батареї (1 година без подій)");
            }


            // 4.5) Фіксуємо снапшот стану для ML
            var snapshot = new WorkstationSnapshot
            {
                SimTime = currentSimTime,
                WorkstationName = workstation.Name,
                McuState = workstation.McuState,

                NetState = workstation.Network.State,
                NetPingMs = workstation.Network.PingLatencyMs,
                NetRetries = workstation.Network.Retries,

                SignalState = workstation.SignalState,

                BatteryHealthState = workstation.Battery.HealthState,
                BatteryStatus = workstation.Battery.Status,
                BatteryPercent = workstation.Battery.ChargePercent,
                BatteryHealthPercent = workstation.Battery.HealthPercent,
                BatteryEffectiveCapacityWh = workstation.Battery.EffectiveCapacityWh,
                BatteryThroughputWh = workstation.Battery.ThroughputWh,

                PowerState = workstation.GetPowerState(),

                ManualWorkingCount = workstation.WorkingManualElements,
                ManualTotalCount = workstation.TotalManualElements,

                AmpOutputPowerWatts = workstation.GetCurrentOutputPower()
            };

            SnapshotLogger.Log(snapshot);

            // 5) Пауза в реальному часі між годинами симуляції
            var realDelay = generator.GetRealDelay(RealSecondsPerSimHour);
            Console.WriteLine(
                $"(Реальна пауза до наступної години: {realDelay.TotalMilliseconds:F0} мс)");
            Thread.Sleep(realDelay);
        }

        WriteSeparator("Симуляцію завершено. Натисніть будь-яку клавішу для виходу...");
        Console.ReadKey();
    
    }

    private static void WriteSeparator(string title)
    {
        Console.WriteLine(new string('=', 70));
        Console.WriteLine(title);
        Console.WriteLine(new string('=', 70));
    }

    private static void WriteIterationHeader(int iteration)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"=== ГОДИНА #{iteration} ===");
        Console.ResetColor();
    }
}
