using WorkstationJobSimulator.Models;

namespace WorkstationJobSimulator.Models.wsModels;

/// <summary>
/// Окремий ручний елемент, що впливає на потужність підсилювача.
/// </summary>
public class ManualElement
{
    public ManualElementState State { get; set; } = ManualElementState.Ok;

    /// "Здоров'я" елемента (0..1). Поки не використовується в розрахунках,
    /// але може знадобитись для моделі деградації.
    public double Health { get; set; } = 1.0;
}
