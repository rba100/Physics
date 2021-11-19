using Physics.Engine;

namespace Physics.Scenarios
{
    internal class StarFormation : Scenario
    {
        public override void Configure(Simulator simulator)
        {
            simulator.Collisions = true;
            simulator.GravityConstant = 1;

            simulator.Particles.Add(new Particle(new Vector3(0, -30, 0), new Vector3(0, 0, 0), 30));
            simulator.Particles.Add(new Particle(new Vector3(0, 30, 0), new Vector3(0, 0, 0), 30));

            simulator.Particles.Add(new Particle(new Vector3(100, 0, 0), new Vector3(0, 0, 0), 30));
        }

        public override string ToString()
        {
            return "Star formation";
        }
    }
}
