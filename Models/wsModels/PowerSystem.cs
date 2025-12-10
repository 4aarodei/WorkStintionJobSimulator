using WorkstationJobSimulator.Models;

namespace WorkstationJobSimulator.Models.wsModels;

public class PowerSystem
{
    // За замовчуванням – живлення від мережі
    public PowerMode Mode { get; set; } = PowerMode.GridOnly;

    public double MainVoltage { get; set; } = 220.0;
    public double BackupVoltage { get; set; } = 12.0;
    public bool IsFuseOk { get; set; } = true;
}
