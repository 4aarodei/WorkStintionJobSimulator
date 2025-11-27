using WorkstationJobSimulator.Events;
using WorkstationJobSimulator.Models.wsModels;

namespace WorkstationJobSimulator.Physic.EventPhysic.Interface;

public interface IEventPhysics
{
    Type EventType { get; }
    void Apply(Workstation ws, SimulationEvent ev);
}
