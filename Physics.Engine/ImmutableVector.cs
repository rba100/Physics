namespace Physics.Engine
{
    public class ImmutableVector : Vector3
    {
        private readonly double m_X;
        private readonly double m_Y;
        private readonly double m_Z;

        public override double X { get { return m_X; } set {; } }
        public override double Y { get { return m_Y; } set {; } }
        public override double Z { get { return m_Z; } set {; } }

        public ImmutableVector(double x, double y, double z)
        {
            m_X = x;
            m_Y = y;
            m_Z = z;
        }
    }
}