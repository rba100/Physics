using Physics.Engine;
using System;

namespace Physics.Scenarios
{
    internal class StarFormation : Scenario
    {
        public override void Configure(Simulator simulator)
        {
            simulator.Collisions = true;
            simulator.GravityConstant = 1;

            var rand = new Random();

            for(int i = 0; i < 1000; i++)
            {
                var r = 400 * Math.Sqrt(rand.NextDouble());
                var a = Math.PI * rand.NextDouble() * 2;

                var position = new Vector3(Math.Cos(a) * r, Math.Sin(a) * r, 0);
                var xVel = a > Math.PI ? 0.05 : -0.05;
                var velocity = new Vector3(xVel, 0, 0);

                simulator.Particles.Add(new Particle(position, velocity, 0.1));
            }
        }

        public override string ToString()
        {
            return "Star formation";
        }
    }

    internal class StarFormation2 : Scenario
    {
        public override void Configure(Simulator simulator)
        {
            simulator.Collisions = true;
            simulator.GravityConstant = 3;
            simulator.StellarIgnitionMass = 200;
            simulator.StellaCollapseMass = 32000;

            var rand = new Random();

            for (int i = 0; i < 2000; i++)
            {
                var r = 4000 * Math.Sqrt(rand.NextDouble());
                var z = 800 * (rand.NextDouble() - 0.5);
                var a = Math.PI * rand.NextDouble() * 2;

                var position = new Vector3(Math.Cos(a) * r, Math.Sin(a) * r, z);
                var xVel = a > Math.PI ? 0.05 : -0.05;
                var velocity = new Vector3(xVel, 0, 0);

                simulator.Particles.Add(new Particle(position, velocity, 20));
            }
        }

        public override string ToString()
        {
            return "Star formation2";
        }
    }
}
