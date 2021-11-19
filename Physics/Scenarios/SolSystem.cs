using Physics.Engine;
using System;

namespace Physics.Scenarios
{
    internal class SolSystem : Scenario
    {
        public override void Configure(Simulator simulator)
        {
            simulator.GravityConstant = 0.2;
            simulator.Collisions = true;
            var unit = Math.Sqrt(0.5);
            var sunMass = 100;
            simulator.Particles.Add(new Particle(new Vector3(0, 0, 0), new Vector3(0, 0, 0), sunMass));

            var mercury = CreatePlanetWithCircularOrbit(sunMass, 50, 1);
            var venus = CreatePlanetWithCircularOrbit(sunMass, 30, 1);
            var earth = CreatePlanetWithCircularOrbit(sunMass, 70, 1);
            var moon = CreateMoon(earth, 5, earth.Mass / 6);
            var mars = CreatePlanetWithCircularOrbit(sunMass, 90, 1);

            var jupiter = CreatePlanetWithCircularOrbit(sunMass, 250, 20);
            var jMoon1 = CreateMoon(jupiter, 10, 0.01);
            var jMoon2 = CreateMoon(jupiter, 20, 0.01);
            var jMoon3 = CreateMoon(jupiter, 30, 0.01);
            var saturn = CreatePlanetWithCircularOrbit(sunMass, 200, 1);
            var urasnus = CreatePlanetWithCircularOrbit(sunMass, 250, 1);
            var neptune = CreatePlanetWithCircularOrbit(sunMass, 300, 1);

            simulator.Particles.Add(mercury);
            simulator.Particles.Add(venus);
            simulator.Particles.Add(earth);
            simulator.Particles.Add(moon);
            //simulator.Particles.Add(mars);
            simulator.Particles.Add(jupiter);
            simulator.Particles.Add(jMoon1);
            simulator.Particles.Add(jMoon2);
            simulator.Particles.Add(jMoon3);
            //simulator.Particles.Add(saturn);
            //simulator.Particles.Add(urasnus);
            //simulator.Particles.Add(neptune);
        }

        public override string ToString()
        {
            return "Sol system";
        }
    }
}
