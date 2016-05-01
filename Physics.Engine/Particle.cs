namespace Physics.Engine
{
    public interface IParticle
    {
        Vector3 Position { get; set; }
        Vector3 Velocity { get; set; }
        double Mass { get; set; }
    }

    public class Particle : IParticle
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public double Mass { get; set; }

        public Particle(Vector3 position, Vector3 velocity, double mass)
        {
            Position = position;
            Velocity = velocity;
            Mass = mass;
        }
    }
}