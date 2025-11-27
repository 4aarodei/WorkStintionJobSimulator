using System.Text;
using WorkstationJobSimulator.Events;
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
    private const int SimulationHours = 1000;

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
