using WorkstationJobSimulator.Models;

namespace WorkstationJobSimulator.Models.wsModels;

public class Battery
{
    /// <summary>Операційний стан батареї (Ok / Cutoff).</summary>
    public BatteryStatus Status { get; set; } = BatteryStatus.Ok;

    /// <summary>Стан здоров'я батареї (Ok / Degraded / Fail).</summary>
    public BatteryHealthState HealthState { get; set; } = BatteryHealthState.Ok;

    /// <summary>Поточний заряд батареї, %.</summary>
    public int ChargePercent { get; set; } = 100;

    /// <summary>Номінальна (паспортна) ємність нової батареї, Wh.</summary>
    public double NominalCapacityWh { get; set; } = 312.0;

    /// <summary>Поточна ефективна ємність з урахуванням деградації, Wh.</summary>
    public double EffectiveCapacityWh { get; set; } = 312.0;

    /// <summary>Скільки Wh сумарно "прокачано" через батарею за весь час.</summary>
    public double ThroughputWh { get; set; } = 0.0;

    /// <summary>State of Health у %, розрахований як Effective / Nominal.</summary>
    public double HealthPercent =>
        NominalCapacityWh <= 0 ? 0 : (EffectiveCapacityWh / NominalCapacityWh) * 100.0;
}
