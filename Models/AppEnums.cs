using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkstationJobSimulator.Models
{
    public enum WorkstationState
    {
        Idle,
        Processing
    }

    public enum PowerMode
    {
        Off = 0,
        GridOnly,
        GridAndCharge,
        Battery
    }

    /// <summary>
    /// Операційний стан батареї по відношенню до Cutoff (живлення/неживлення).
    /// </summary>
    public enum BatteryStatus
    {
        Ok = 0,
        Cutoff
    }

    public enum BatteryHealthState
    {
        Ok,
        Degraded,
        Fail,
        FailUnderLoad   // ← додати цей!
    }


    public enum AmplifierStatus
    {
        Off = 0,
        On,
        Fault
    }

    public enum NetState
    {
        Normal = 0,
        HighLatency,
        Offline,
        ControllerFailure
    }

    public enum NetworkChannel
    {
        Reserve = 0,
        Main = 1
    }

    /// <summary>Стан мікроконтролера.</summary>
    public enum McuState
    {
        Ok = 0,
        Fail = 1
    }

    /// <summary>Стан вхідного сигналу підсилювача.</summary>
    public enum SignalState
    {
        Ok = 0,
        Fail = 1
    }

    /// <summary>Стан окремого ручного елемента.</summary>
    public enum ManualElementState
    {
        Ok = 0,
        Fail = 1
    }

    /// <summary>Узагальнений стан джерела живлення.</summary>
    public enum PowerState
    {
        Ac = 0,      // живлення від мережі 220В
        Dc = 1,      // живлення від батареї
        NoPower = 2  // живлення відсутнє
    }
}
