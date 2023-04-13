using System;
using System.Collections.Generic;
using System.Threading;

namespace Physics.Engine
{
    public class Simulator : IDisposable
    {
        public List<Particle> Particles { get; set; } = new List<Particle>();

        public int TickInterval { get; set; } = 10;
        public double GravityConstant { get; set; } = 1;
        public bool Collisions { get; set; }
        public double StellarIgnitionMass { get; set; } = 50;
        public double StellaCollapseMass { get; set; } = 1500;

        public double MaxPhysicsDistance = 2000;

        public event EventHandler<EventArgs> Tick;
        public event EventHandler<MergeEventArgs> ParticlesMerged;

        private Thread WorkerThread;
        private bool isDisposed;

        public void Start()
        {
            if (WorkerThread != null) return;
            WorkerThread = new Thread(Worker);
            WorkerThread.Start();
        }

        private void Worker()
        {
            var args = new EventArgs();
            while (!isDisposed)
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
            isDisposed = true;
            WorkerThread?.Join(TimeSpan.FromSeconds(10));
            WorkerThread = null;
        }

        private void ApplyForces()
        {
            ProcessParticles((a, b) => ApplyMutualGravity(a, b));
        }

        private void ApplyMutualGravity(Particle a, Particle b)
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
            ProcessParticles((a, b) =>
            {
                var displacement = (a.Position - b.Position).Magnitude;
                if (displacement > MaxPhysicsDistance) return;
                if (Math.Sqrt(a.Mass + b.Mass) / displacement > 1)
                {
                    var merged = MergeParticles(a, b);
                    Particles.Insert(Particles.IndexOf(a), merged);
                    Particles.Remove(a);
                    Particles.Remove(b);
                }
            });
        }

        private Particle MergeParticles(Particle a, Particle b)
        {
            var mass = a.Mass + b.Mass;
            var velocity = (a.Velocity.WithScale(a.Mass) + b.Velocity.WithScale(b.Mass)).WithScale(1 / mass);
            var position = (a.Position.WithScale(a.Mass) + b.Position.WithScale(b.Mass)).WithScale(1 / mass);
            var particle = new Particle(position, velocity, mass);
            ParticlesMerged?.Invoke(this, new MergeEventArgs { A = a, B = b, Merged = particle });
            return particle;
        }

        private void Move()
        {
            foreach (var particle in Particles)
            {
                particle.Position.Accumulate(particle.Velocity);
            }
        }

        private void ProcessParticles(Action<Particle, Particle> action)
        {
            var particles = Particles.ToArray();

            for (int i = 0; i < particles.Length - 1; i++)
            {
                for (int j = i + 1; j < particles.Length; j++)
                {
                    action(particles[i], particles[j]);
                }
            }
        }

        private double ForceFromGravity(Particle a, Particle b)
        {
            var distance = (b.Position - a.Position).Magnitude;
            return GravityConstant * (a.Mass * b.Mass) / (distance * distance);
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public class MergeEventArgs : EventArgs
    {
        public Particle Merged;
        public Particle A { get; set; }
        public Particle B { get; set; }
    }
}
