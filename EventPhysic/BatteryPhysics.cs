using WorkstationJobSimulator.Models;
using WorkstationJobSimulator.Models.wsModels;

namespace WorkstationJobSimulator.Physics;

public class BatteryPhysics
{
    private const double BatteryCapacityWh = 1000.0; // TODO: винести в конфіг

    public void ConsumeEnergy(Workstation ws, double powerWatts, TimeSpan duration, string reason)
    {
        var battery = ws.Battery;

        var energyWh = powerWatts * duration.TotalHours;
        var percentDrop = energyWh / BatteryCapacityWh * 100.0;
        var delta = (int)Math.Round(percentDrop);

        var oldPercent = battery.ChargePercent;
        battery.ChargePercent = Math.Max(0, oldPercent - delta);

        ws.LogState(
            $"Батарея витратила {delta}% на \"{reason}\". " +
            $"Було {oldPercent}%, стало {battery.ChargePercent}%.");

        if (battery.ChargePercent <= 0)
        {
            battery.Status = BatteryStatus.Cutoff;
            ws.SetPower(false, "Батарея розряджена");
            ws.LogState("Батарея в режимі Cutoff, робоча станція знеструмлена.");
        }
    }

    public void Charge(Workstation ws, double chargePowerWatts, TimeSpan duration, string reason)
    {
        var battery = ws.Battery;

        var energyWh = chargePowerWatts * duration.TotalHours;
        var percentGain = energyWh / BatteryCapacityWh * 100.0;
        var delta = (int)Math.Round(percentGain);

        var oldPercent = battery.ChargePercent;
        battery.ChargePercent = Math.Min(100, oldPercent + delta);

        ws.LogState(
            $"Батарея заряджена на {delta}% завдяки \"{reason}\". " +
            $"Було {oldPercent}%, стало {battery.ChargePercent}%.");

        if (battery.ChargePercent > 0 && battery.Status == BatteryStatus.Cutoff)
        {
            battery.Status = BatteryStatus.Ok;
            ws.SetPower(true, "Живлення відновлено – батарея частково заряджена");
        }
    }
}
