using Physics.Engine;

namespace Physics.Scenarios
{
    internal class Trisolaris : Scenario
    {
        public override void Configure(Simulator simulator)
        {
            simulator.Collisions = true;

            simulator.GravityConstant = 1;
            
            var starMass = 100;

            double scale = 100;

            double initRx = -0.97000436;
            double initRy = 0.24308753;

            double initV1x = 0.4662036850;
            double initV1y = 0.4323657300;

            double initV2x = -0.93240737;
            double initV2y = -0.86473146;

            var starPos1 = new Vector3(initRx * scale, initRy * scale, 0);
            var starPos2 = new Vector3(0, 0, 0);
            var starPos3 = starPos1.Inverse();

            var starVel1 = new Vector3(initV1x, initV1y, 0);
            var starVel2 = new Vector3(initV2x, initV2y, 0);
            var starVel3 = new Vector3(initV1x, initV1y, 0);

            var star1 = new Particle(starPos1, starVel1, starMass);
            var star2 = new Particle(starPos2, starVel2, starMass);
            var star3 = new Particle(starPos3, starVel3, starMass);

            simulator.Particles.Add(star1);
            simulator.Particles.Add(star2);
            simulator.Particles.Add(star3);

            var planet = CreateMoon(new Particle(Vector3.Zero, Vector3.Zero, 3 * starMass), 500, 1, simulator.GravityConstant);

            simulator.Particles.Add(planet);
        }

        private double GetSpeed(double displacement, double total)
        {
            return total / (displacement * displacement);
        }

        public override string ToString()
        {
            return "Trisolaris";
        }
    }
}
