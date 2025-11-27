using WorkstationJobSimulator.Models;
using WorkstationJobSimulator.Models.wsModels;

namespace WorkstationJobSimulator.Physic;

public class BatteryPhysics
{
    public void ConsumeEnergy(Workstation ws, double powerWatts, TimeSpan duration, string reason)
    {
        var battery = ws.Battery;
        var capacityWh = battery.CapacityWh;

        if (capacityWh <= 0)
        {
            ws.LogState("[WARN] Ємність батареї <= 0, розрахунок споживання неможливий.");
            return;
        }

        var energyWh = powerWatts * duration.TotalHours;
        var percentDrop = energyWh / capacityWh * 100.0;

        // Щоб дрібні витрати (типу 1.6%) не зникали:
        var delta = (int)Math.Round(percentDrop);
        if (delta <= 0 && percentDrop > 0)
            delta = 1;

        var oldPercent = battery.ChargePercent;
        battery.ChargePercent = Math.Max(0, oldPercent - delta);

        ws.LogState(
            $"Батарея витратила {oldPercent - battery.ChargePercent}% на \"{reason}\". " +
            $"Було {oldPercent}%, стало {battery.ChargePercent}%.");

        if (battery.ChargePercent <= 0)
        {
            battery.Status = BatteryStatus.Cutoff;
            ws.SetPower(false, "Батарея розряджена");
            ws.LogState("Батарея в режимі Cutoff, робоча станція знеструмлена.");
        }
    }

    public void ChargeBattery(Workstation ws, double powerWatts, TimeSpan duration, string reason)
    {
        var battery = ws.Battery;
        var capacityWh = battery.CapacityWh;

        if (battery.ChargePercent >= 100)
        {
            ws.LogState($"Батарея вже повністю заряджена – заряджання від \"{reason}\" не потрібне.");
            return;
        }

        if (capacityWh <= 0)
        {
            ws.LogState("[WARN] Ємність батареї <= 0, розрахунок заряджання неможливий.");
            return;
        }

        var energyWh = powerWatts * duration.TotalHours;
        var percentGain = energyWh / capacityWh * 100.0;

        var delta = (int)Math.Round(percentGain);
        if (delta <= 0 && percentGain > 0)
            delta = 1;

        var oldPercent = battery.ChargePercent;
        battery.ChargePercent = Math.Min(100, oldPercent + delta);

        ws.LogState(
            $"Батарея заряджена на {battery.ChargePercent - oldPercent}% завдяки \"{reason}\". " +
            $"Було {oldPercent}%, стало {battery.ChargePercent}%.");

        if (battery.ChargePercent > 0 && battery.Status == BatteryStatus.Cutoff)
        {
            battery.Status = BatteryStatus.Ok;
            ws.SetPower(true, "Живлення відновлено – батарея частково заряджена");
        }
    }
}
