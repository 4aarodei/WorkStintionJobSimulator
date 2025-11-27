using System.Text;
using System.IO;

namespace WorkstationJobSimulator;

public static class SimulationLogger
{
    /// <summary>
    /// Налаштовує дублювання всього консольного виводу у текстовий файл.
    /// Повертає шлях до лог-файлу.
    /// </summary>
    public static string Setup()
    {
        // ТУТ задаємо фіксовану папку для логів
        var logDir = @"D:\Programing\Projects\c#\WorkStintionJobSimulator\logs\";
        Directory.CreateDirectory(logDir);

        var logFilePath = Path.Combine(
            logDir,
            $"simulation_{DateTime.Now:dd.MM.y_HH-mm}.txt");

        var fileWriter = new StreamWriter(logFilePath, append: false, Encoding.UTF8)
        {
            AutoFlush = true
        };

        var teeWriter = new TeeTextWriter(Console.Out, fileWriter);
        Console.SetOut(teeWriter);

        return logFilePath;
    }

    /// <summary>
    /// TextWriter, який дублює весь вивід і в консоль, і у файл.
    /// </summary>
    private sealed class TeeTextWriter : TextWriter
    {
        private readonly TextWriter _consoleWriter;
        private readonly TextWriter _fileWriter;

        public TeeTextWriter(TextWriter consoleWriter, TextWriter fileWriter)
        {
            _consoleWriter = consoleWriter;
            _fileWriter = fileWriter;
        }

        public override Encoding Encoding => _consoleWriter.Encoding;

        public override void Write(char value)
        {
            _consoleWriter.Write(value);
            _fileWriter.Write(value);
        }

        public override void Write(string? value)
        {
            _consoleWriter.Write(value);
            _fileWriter.Write(value);
        }

        public override void WriteLine()
        {
            _consoleWriter.WriteLine();
            _fileWriter.WriteLine();
        }

        public override void WriteLine(string? value)
        {
            _consoleWriter.WriteLine(value);
            _fileWriter.WriteLine(value);
        }

        public override void Flush()
        {
            _consoleWriter.Flush();
            _fileWriter.Flush();
            base.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _consoleWriter.Flush();
                _fileWriter.Flush();
                _fileWriter.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
