using Physics.Engine;
using System;

namespace Physics.Scenarios
{
    internal abstract class Scenario
    {
        public abstract void Configure(Simulator simulator);

        protected Particle CreatePlanetWithCircularOrbit(double sunMass, double altitude, double planetMass, double gravityConstant = 0.2)
        {
            var planetPosition = new Vector3(altitude, 0, 0);
            var planetSpeed = CircularOrbitSpeed(sunMass + planetMass, altitude, gravityConstant);
            var planet = new Particle(planetPosition, new Vector3(0, -planetSpeed, 0), planetMass);
            return planet;
        }

        protected Particle CreateMoon(Particle parent, double altitude, double mass, double gravityConstant = 0.2)
        {
            var moonPosition = new Vector3(parent.Position.Magnitude + altitude, 0, 0);
            var moonSpeed = CircularOrbitSpeed(mass + parent.Mass, altitude, gravityConstant) + parent.Velocity.Magnitude;
            return new Particle(moonPosition, new Vector3(0, -moonSpeed, 0), mass);
        }

        private double CircularOrbitSpeed(double totalMass, double radius, double gravityConstant)
        {
            return Math.Sqrt(gravityConstant * totalMass / radius);
        }
    }
}
