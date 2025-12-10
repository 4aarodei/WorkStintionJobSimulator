using System;
using System.IO;
using System.Text;

namespace WorkstationJobSimulator.Logers;

public static class SimulationLogger
{
    public static string Setup()
    {
        var logDir = @"D:\Programing\Projects\c#\WorkStintionJobSimulator\logs";
        Directory.CreateDirectory(logDir);

        string fileName = $"simulation_{DateTime.Now:dd.MM.yyyy_HH-mm}.txt";
        string filePath = Path.Combine(logDir, fileName);

        var writer = new DualWriter(Console.Out, new StreamWriter(filePath, append: true, Encoding.UTF8));
        Console.SetOut(writer);

        return filePath;
    }

    private class DualWriter : TextWriter
    {
        private readonly TextWriter console;
        private readonly TextWriter file;

        public DualWriter(TextWriter console, TextWriter file)
        {
            this.console = console;
            this.file = file;
        }

        public override Encoding Encoding => console.Encoding;

        public override void WriteLine(string? value)
        {
            if (value != null)
            {
                if (value.Contains("SAG", StringComparison.OrdinalIgnoreCase))
                    value = "[VOLTAGE_SAG] " + value;

                if (value.Contains("FailUnderLoad", StringComparison.OrdinalIgnoreCase))
                    value = "[FAIL_UNDER_LOAD] " + value;

                if (value.Contains("Деградація"))
                    value = "[DEGRADATION] " + value;
            }

            console.WriteLine(value);
            file.WriteLine(value);
            file.Flush();
        }
    }
}
