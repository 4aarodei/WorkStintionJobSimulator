using System.Text;
using WorkstationJobSimulator.Events;
using WorkstationJobSimulator.Models.wsModels;
using WorkstationJobSimulator.PhysicsRegistration;

namespace WorkstationJobSimulator;

public static class Program
{
    /// <summary>
    /// Кількість подій у симуляції.
    /// </summary>
    private const int SimulationIterations = 20;

    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var workstation = new Workstation("WS-01");
        var generator = new SimulationEventGenerator();

        var physicsEngine = new WorkstationPhysicsEngine();
        RegistrationPhysicsMaster.RegisterAllEventPhysics(physicsEngine);

        WriteSeparator("Починаємо симуляцію робочої станції...");

        for (int i = 0; i < SimulationIterations; i++)
        {
            WriteIterationHeader(i + 1);

            // 1) Генеруємо випадкову подію
            var ev = generator.Generate();
            Console.WriteLine($"[EVENT] {ev.EventName}, тривалість {ev.Duration}");

            // 2) Виконуємо подію: всередині вона делегує логіку у WorkstationPhysicsEngine
            ev.Execute(workstation, physicsEngine);

            // 3) Показуємо стан після події
            Console.WriteLine();
            Console.WriteLine("[STATE] Стан станції після обробки події:");
            workstation.PrintStatus();

            // 4) Інтервал до наступної події
            var delay = generator.RollNextInterval();
            Console.WriteLine($"(Пауза до наступної події: {delay.TotalSeconds:F0} c)");
            Thread.Sleep(delay);
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
        Console.WriteLine($"=== ІТЕРАЦІЯ #{iteration} ===");
        Console.ResetColor();
    }
}
