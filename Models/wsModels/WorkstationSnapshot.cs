using System;
using WorkstationJobSimulator.Models;
using WorkstationJobSimulator.Models.wsModels;

namespace WorkstationJobSimulator.Models.wsModels
{
    public class WorkstationSnapshot
    {
        public DateTime SimTime { get; set; }
        public string WorkstationName { get; set; } = string.Empty;

        // MCU
        public McuState McuState { get; set; }

        // SIGNAL
        public SignalState SignalState { get; set; }

        // NETWORK
        public NetState NetState { get; set; }
        public int NetPingMs { get; set; }
        public int NetRetries { get; set; }

        // ENVIRONMENT
        public double AmbientTemp { get; set; }

        // BATTERY BASIC METRICS
        public BatteryHealthState BatteryHealthState { get; set; }
        public BatteryStatus BatteryStatus { get; set; }
        public int BatteryPercent { get; set; }
        public double BatteryHealthPercent { get; set; }
        public double BatteryEffectiveCapacityWh { get; set; }
        public double BatteryThroughputWh { get; set; }

        // BATTERY ADVANCED
        public bool BatteryVoltageSag { get; set; }
        public bool BatteryFailUnderLoad { get; set; }
        public double BatteryTempFactor { get; set; }
        public double BatteryEnergyDeltaWh { get; set; }
        public double BatteryEffectiveCapacityDeltaWh { get; set; }

        // POWER
        public PowerState PowerState { get; set; }

        // MANUAL CONTROL
        public int ManualWorkingCount { get; set; }
        public int ManualTotalCount { get; set; }

        // AMPLIFIER
        public double AmpOutputPowerWatts { get; set; }
    }
}
