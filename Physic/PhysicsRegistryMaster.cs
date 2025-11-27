using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkstationJobSimulator.Physic.EventPhysic;

namespace WorkstationJobSimulator.Physic
{
    public static class RegistrationPhysicsMaster
    {
        public static void RegisterAllEventPhysics(WorkstationPhysicsEngine engine)
        {
            // Тут ти реєструєш всі реалізації IEventPhysics
            engine.Register(new AirAlarmPhysics());
            engine.Register(new TurningOffTheLightsPhysics());
            // Коли зʼявляться нові:
            // engine.Register(new OverheatPhysics());

        }
    }
}
