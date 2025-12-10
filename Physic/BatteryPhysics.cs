using System;
using WorkstationJobSimulator.Models;
using WorkstationJobSimulator.Models.wsModels;

namespace WorkstationJobSimulator.Physic
{
    public class BatteryPhysics
    {
        private readonly Random _rand = new();

        // Паспортні параметри
        private const double PassportCycles = 500.0; // Ресурс циклів до SoH=80%
        private const double SoHEol = 0.80;          // 80% SoH в кінці ресурсу

        // Фізичні параметри
        private const double OptimalTemp = 25.0;     // Оптимальна робоча температура
        private const double MinVoltage = 10.0;      // Напруга "мертвої" батареї
        private const double NominalVoltage = 12.8;  // Напруга при 100%


        // ======================================================
        // 🔥 РОЗРЯДЖАННЯ БАТАРЕЇ
        // ======================================================
        public void ConsumeEnergy(Workstation ws, double powerWatts, TimeSpan duration, string reason)
        {
            var battery = ws.Battery;

            if (battery.EffectiveCapacityWh <= 0)
            {
                ws.LogState("[WARN] EffectiveCapacityWh <= 0 – батарея не може віддати енергію.");
                return;
            }

            double energyWh = powerWatts * duration.TotalHours;

            // Додати в статистику "зношення"
            battery.ThroughputWh += energyWh;

            // Нелінійна ефективність залежно від температури
            double tempFactor = TemperatureFactor(ws.AmbientTemp);

            // Скільки % заряду реально втратилось
            double dropPercent = (energyWh / (battery.EffectiveCapacityWh * tempFactor)) * 100.0;

            // Мінімальний крок — 1%
            int delta = (int)Math.Round(dropPercent);
            if (delta <= 0 && dropPercent > 0) delta = 1;

            int oldPercent = battery.ChargePercent;
            battery.ChargePercent = Math.Max(0, battery.ChargePercent - delta);

            ws.LogState(
                $"Батарея витратила {oldPercent - battery.ChargePercent}% ({energyWh:F1} Wh) на \"{reason}\". " +
                $"Було {oldPercent}%, стало {battery.ChargePercent}%.");

            // Симптоми старіння — Voltage sag, FailUnderLoad
            ApplyFailureSymptoms(ws, battery, powerWatts);

            // Мертва батарея
            if (battery.ChargePercent <= 0)
            {
                battery.Status = BatteryStatus.Cutoff;
                ws.SetPower(false, "Батарея розряджена");
                ws.LogState("Батарея в режимі Cutoff — станція знеструмлена.");
            }

            ApplyDegradation(battery, ws);
        }


        // ======================================================
        // 🔥 ЗАРЯДЖАННЯ БАТАРЕЇ
        // ======================================================
        public void ChargeBattery(Workstation ws, double powerWatts, TimeSpan duration, string reason)
        {
            var battery = ws.Battery;

            if (battery.ChargePercent >= 100)
            {
                ws.LogState("Батарея вже повністю заряджена.");
                return;
            }

            if (battery.EffectiveCapacityWh <= 0)
            {
                ws.LogState("[WARN] EffectiveCapacityWh <= 0 — заряджання неможливе.");
                return;
            }

            double energyWh = powerWatts * duration.TotalHours;

            double gainPercent = (energyWh / battery.EffectiveCapacityWh) * 100.0;

            int delta = (int)Math.Round(gainPercent);
            if (delta <= 0 && gainPercent > 0) delta = 1;

            int oldPercent = battery.ChargePercent;
            battery.ChargePercent = Math.Min(100, battery.ChargePercent + delta);

            battery.ThroughputWh += energyWh;

            ws.LogState(
                $"Батарея заряджена на {battery.ChargePercent - oldPercent}% ({energyWh:F1} Wh) – \"{reason}\". " +
                $"Було {oldPercent}%, стало {battery.ChargePercent}%.");

            // Вихід із Cutoff
            if (battery.Status == BatteryStatus.Cutoff && battery.ChargePercent > 5)
            {
                battery.Status = BatteryStatus.Ok;
                ws.SetPower(true, "Живлення відновлено – батарея частково заряджена.");
            }

            ApplyDegradation(battery, ws);
        }



        // ======================================================
        // 🔥 НЕ ЛІНІЙНА ДЕГРАДАЦІЯ SOH
        // ======================================================
        private void ApplyDegradation(Battery battery, Workstation ws)
        {
            double totalWhToEol = PassportCycles * battery.NominalCapacityWh;
            double usedFraction = battery.ThroughputWh / totalWhToEol;

            // Нелінійна крива: швидке старіння на кінці ресурсу
            double curve = 0.7 * usedFraction + 0.3 * Math.Sqrt(usedFraction);
            curve = Math.Min(curve, 1.0);

            double newEffective = battery.NominalCapacityWh * (1 - curve * (1 - SoHEol));
            double oldEffective = battery.EffectiveCapacityWh;

            battery.EffectiveCapacityWh = Math.Max(0, newEffective);

            // Оновити HealthState
            double soh = battery.HealthPercent;
            if (soh > 70) battery.HealthState = BatteryHealthState.Ok;
            else if (soh > 30) battery.HealthState = BatteryHealthState.Degraded;
            else battery.HealthState = BatteryHealthState.Fail;

            if (Math.Abs(oldEffective - battery.EffectiveCapacityWh) > 0.05)
            {
                ws.LogState(
                    $"Деградація: {oldEffective:F1}Wh → {battery.EffectiveCapacityWh:F1}Wh " +
                    $"(SoH={battery.HealthPercent:F1}%, Throughput={battery.ThroughputWh:F1}Wh).");
            }
        }


        // ======================================================
        // 🔥 ПЕРЕДВІСНИКИ ВІДМОВИ (FailUnderLoad / VoltageSag)
        // ======================================================
        private void ApplyFailureSymptoms(Workstation ws, Battery battery, double powerWatts)
        {
            // Симуляція Voltage SAG
            if (battery.ChargePercent < 20 && _rand.NextDouble() < 0.05)
            {
                ws.LogState("[WARN] Просадка напруги – стара батарея не тримає навантаження.");
            }

            // Fail under load (реальний симптом вмираючого АКБ)
            if (powerWatts > 50 && battery.ChargePercent < 15 && _rand.NextDouble() < 0.02)
            {
                battery.HealthState = BatteryHealthState.FailUnderLoad;
                ws.LogState("[ERROR] Батарея не витримала навантаження → FailUnderLoad.");
            }
        }


        // ======================================================
        // 🔥 Температурний коефіцієнт (безпечна модель)
        // ======================================================
        private double TemperatureFactor(double temp)
        {
            // Фізика:
            // 0°C → ~70%
            // -10°C → ~50%
            // -20°C → ~30%

            if (temp >= OptimalTemp) return 1.0;

            double delta = OptimalTemp - temp;

            // плавне падіння ефективності
            return Math.Max(0.3, 1.0 - delta * 0.03);
        }
    }
}
