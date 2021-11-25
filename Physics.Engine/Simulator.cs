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
        public double StellarIgnitionMass { get; set; } = 50;
        public double StellaCollapseMass { get; set; } = 1500;

        public double MaxPhysicsDistance = 2000;

        public delegate void MargeEventHandler(object sender, MergeEventArgs args);
        public event MargeEventHandler ParticlesMerged;

        public event EventHandler Tick;

        private Thread m_Worker;

        private bool m_Disposed;

        public void Start()
        {
            if (m_Worker != null) return;
            m_Worker = new Thread(Worker);
            m_Worker.Start();
        }

        private void Worker()
        {
            var args = new EventArgs();
            while (!m_Disposed)
            {
                Thread.Sleep(TickInterval);
                ApplyForces();
                ApplyCollisions();
                Move();
                Tick?.Invoke(this, args);
            }
        }

        private void Stop()
        {
            m_Disposed = true;
            if (m_Worker == null) return;
            m_Worker.Join(TimeSpan.FromSeconds(10));
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
            var AtoB = b.Position - a.Position;
            if (AtoB.Magnitude > MaxPhysicsDistance) return;
            var force = AtoB.UnitVector().WithScale(ForceFromGravity(a, b));

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
                    if (displacement > MaxPhysicsDistance) continue;
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

        private double ForceFromGravity(IParticle a, IParticle b)
        {
            var d = (b.Position - a.Position).Magnitude;
            return GravityConstant * (a.Mass * b.Mass) / (d * d);
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
