using Physics.Engine;
using System;

namespace Physics.Scenarios
{
    internal class Unstable3D : Scenario
    {
        public override void Configure(Simulator simulator)
        {
            simulator.GravityConstant = 15;

            simulator.Particles.Add(new Particle(new Vector3(0, -30, 0), new Vector3(-3, 0, 0), 100));
            simulator.Particles.Add(new Particle(new Vector3(0, 30, 0), new Vector3(3, 0, 0), 100));
            simulator.Particles.Add(new Particle(new Vector3(0, 0, -200), new Vector3(-3, 0, 0), 20));
            simulator.Particles.Add(new Particle(new Vector3(0, 0, 200), new Vector3(3, 0, 0), 20));
            simulator.Particles.Add(new Particle(new Vector3(0, -200, 0), new Vector3(0, 0, -3), 20));
            simulator.Particles.Add(new Particle(new Vector3(0, 200, 0), new Vector3(0, 0, 3), 20));
        }

        public override string ToString()
        {
            return "3D madness";
        }
    }
}
