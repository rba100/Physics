using Physics.Engine;

namespace Physics.Scenarios
{
    internal class BinaryWithPlanet : Scenario
    {
        public override void Configure(Simulator simulator)
        {
            simulator.GravityConstant = 2;

            simulator.Particles.Add(new Particle(new Vector3(0, -30, 0), new Vector3(-1.3, 0, 0), 100));
            simulator.Particles.Add(new Particle(new Vector3(0, 30, 0), new Vector3(1.3, 0, 0), 100));

            simulator.Particles.Add(new Particle(new Vector3(0, -200, 0), new Vector3(1, 0, 0), 1));
        }

        public override string ToString()
        {
            return "Binary with planet";
        }
    }
}
