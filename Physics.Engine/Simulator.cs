using System;
using System.Collections.Generic;
using System.Threading;

namespace Physics.Engine
{
    public class Simulator : IDisposable
    {
        public List<IParticle> Particles { get; set; } = new List<IParticle>();

        public int TickInterval { get; set; } = 10;
        public double GravityConstant { get; set; } = 1;
        public bool Collisions { get; set; }

        public delegate void MargeEventHandler(object sender, MergeEventArgs args);
        public event MargeEventHandler ParticlesMerged;

        public event EventHandler Tick;

        private Thread m_Worker;

        public void Start()
        {
            if (m_Worker != null) return;
            m_Worker = new Thread(Worker);
            m_Worker.Start();
        }

        private void Worker()
        {
            var args = new EventArgs();
            while (true)
            {
                Thread.Sleep(TickInterval);
                ApplyForces();
                ApplyCollisions();
                Move();
                Tick?.Invoke(this, args);
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
            var AtoB = (b.Position - a.Position);
            var force = AtoB.UnitVector().WithScale(ForceFromGravity(a, b, AtoB.Magnitude));

            var aAccelleration = (new Vector3(force)).WithScale(1 / a.Mass);
            var bAccelleration = (force.Inverse()).WithScale(1 / b.Mass);

            a.Velocity.Accumulate(aAccelleration);
            b.Velocity.Accumulate(bAccelleration);
        }

        private void ApplyCollisions()
        {
            if (!Collisions) return;
            var particles = Particles.ToArray();

            // Iterate over all pairs
            for (int i = 0; i < particles.Length - 1; i++)
            {
            repeat:
                for (int j = i + 1; j < particles.Length; j++)
                {
                    var a = particles[i];
                    var b = particles[j];
                    var displacement = (a.Position - b.Position).Magnitude;
                    if (Math.Sqrt(a.Mass + b.Mass) / displacement > 1)
                    {
                        var merged = MergeParticles(a, b);
                        Particles.Insert(i, merged);
                        Particles.Remove(a);
                        Particles.Remove(b);
                        particles = Particles.ToArray();
                        goto repeat; // Oh no I didn't.
                    }
                }
            }
        }

        private IParticle MergeParticles(IParticle a, IParticle b)
        {
            var mass = a.Mass + b.Mass;
            var veolcity = (a.Velocity.WithScale(a.Mass) + b.Velocity.WithScale(b.Mass)).WithScale(1 / mass);
            var position = (a.Position.WithScale(a.Mass) + b.Position.WithScale(b.Mass)).WithScale(1 / mass);
            var particle = new Particle(position, veolcity, mass);
            ParticlesMerged?.Invoke(this, new MergeEventArgs { A = a, B = b, Merged = particle });
            return particle;
        }

        private double ForceFromGravity(IParticle a, IParticle b, double displacement)
        {
            return GravityConstant * (a.Mass * b.Mass) / (displacement * displacement);
        }

        private void Move()
        {
            foreach (var particle in Particles)
            {
                particle.Position.Accumulate(particle.Velocity);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public class MergeEventArgs : EventArgs
    {
        public IParticle Merged;
        public IParticle A { get; set; }
        public IParticle B { get; set; }
    }
}
