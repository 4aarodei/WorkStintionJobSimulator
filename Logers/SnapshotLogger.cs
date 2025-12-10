using System;
using System.IO;
using System.Text;
using WorkstationJobSimulator.Models.wsModels;

namespace WorkstationJobSimulator.Logers
{
    public static class SnapshotLogger
    {
        private static readonly object _lock = new();
        private static string? _filePath;
        private static bool _headerWritten = false;

        public static string Setup()
        {
            var logDir = @"D:\Programing\Projects\c#\WorkStintionJobSimulator\logs";
            Directory.CreateDirectory(logDir);

            string fileName = $"snapshots_{DateTime.Now:dd.MM.yyyy_HH-mm}.csv";
            _filePath = Path.Combine(logDir, fileName);
            _headerWritten = false;

            return _filePath;
        }

        public static void Log(WorkstationSnapshot s)
        {
            if (_filePath == null)
                throw new InvalidOperationException("Call SnapshotLogger.Setup() before Log().");

            lock (_lock)
            {
                using var sw = new StreamWriter(_filePath, append: true, Encoding.UTF8);

                if (!_headerWritten)
                {
                    sw.WriteLine(
                        "SimTime,Workstation," +
                        "McuState,SignalState," +
                        "NetState,NetPingMs,NetRetries," +
                        "AmbientTemp," +
                        "BatteryHealthState,BatteryStatus,BatteryPercent,BatteryHealthPercent," +
                        "BatteryEffectiveCapacityWh,BatteryThroughputWh," +
                        "BatteryVoltageSag,BatteryFailUnderLoad," +
                        "BatteryTempFactor,BatteryEnergyDeltaWh,BatteryEffectiveCapacityDeltaWh," +
                        "PowerState,ManualWorkingCount,ManualTotalCount," +
                        "AmpOutputPowerWatts"
                    );

                    _headerWritten = true;
                }

                sw.WriteLine(string.Join(",", new[]
                {
                    s.SimTime.ToString("O"),
                    s.WorkstationName,

                    s.McuState.ToString(),
                    s.SignalState.ToString(),

                    s.NetState.ToString(),
                    s.NetPingMs.ToString(),
                    s.NetRetries.ToString(),

                    s.AmbientTemp.ToString("F2"),

                    s.BatteryHealthState.ToString(),
                    s.BatteryStatus.ToString(),
                    s.BatteryPercent.ToString(),
                    s.BatteryHealthPercent.ToString("F2"),
                    s.BatteryEffectiveCapacityWh.ToString("F2"),
                    s.BatteryThroughputWh.ToString("F2"),

                    s.BatteryVoltageSag ? "1" : "0",
                    s.BatteryFailUnderLoad ? "1" : "0",

                    s.BatteryTempFactor.ToString("F3"),
                    s.BatteryEnergyDeltaWh.ToString("F3"),
                    s.BatteryEffectiveCapacityDeltaWh.ToString("F3"),

                    s.PowerState.ToString(),
                    s.ManualWorkingCount.ToString(),
                    s.ManualTotalCount.ToString(),

                    s.AmpOutputPowerWatts.ToString("F2")
                }));
            }
        }
    }
}
