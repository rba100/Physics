using System.Collections.Generic;
using System.Threading;

namespace Physics.Engine
{
    public class Simulator
    {
        public List<IParticle> Particles { get; set; } = new List<IParticle>();

        public int TickInterval { get; set; } = 10;
        public double GravityConstant { get; set; } = 1;
        
        private Thread m_Worker;

        public void Start()
        {
            if (m_Worker != null) return;
            m_Worker = new Thread(Worker);
            m_Worker.Start();
        }

        private void Worker()
        {
            while (true)
            {
                Thread.Sleep(TickInterval);
                ApplyForces();
                Move();
            }
        }

        public void Stop()
        {
            if (m_Worker == null) return;
            m_Worker.Abort();
            m_Worker = null;
        }

        private void ApplyForces()
        {
            var particles = Particles.ToArray();

            // Iterate over all pairs
            for (int i = 0; i < particles.Length - 1; i++)
            {
                for (int j = i + 1; j < particles.Length; j++)
                {
                    ApplyMutualGravity(particles[i], particles[j]);
                }
            }
        }

        private void ApplyMutualGravity(IParticle a, IParticle b)
        {
            var force = (b.Position - a.Position).UnitVector().WithScale(ForceFromGravity(a, b));

            var aAccelleration = (new Vector3(force)).WithScale(1 / a.Mass);
            var bAccelleration = (new Vector3(force.Inverse())).WithScale(1 / b.Mass);

            a.Velocity.Accumulate(aAccelleration);
            b.Velocity.Accumulate(bAccelleration);
        }

        private double ForceFromGravity(IParticle a, IParticle b)
        {
            var displacement = (b.Position - a.Position).Magnitude;
            return GravityConstant * (a.Mass * b.Mass) / (displacement * displacement);
        }

        private void Move()
        {
            foreach (var particle in Particles)
            {
                particle.Position.Accumulate(particle.Velocity);
            }
        }
    }
}
