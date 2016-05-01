namespace Physics.Engine
{
    public class FixedBody : IParticle
    {
        private readonly ImmutableVector m_Zero = new ImmutableVector(0, 0, 0);
        private readonly ImmutableVector m_Position;

        public Vector3 Position { get { return m_Position; } set {; } }
        public Vector3 Velocity { get { return m_Zero; } set {; } }
        public double Mass { get; set; }

        public FixedBody(Vector3 position, double mass)
        {
            Mass = mass;
            m_Position = new ImmutableVector(position.X, position.Y, position.Z);
        }
    }
}