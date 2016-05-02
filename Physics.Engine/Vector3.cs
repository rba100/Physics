using System;
using System.Security.Claims;

namespace Physics.Engine
{
    public class Vector3
    {
        public double X;
        public double Y;
        public double Z;

        public Vector3()
        {
        }

        public Vector3(Vector3 other)
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
        }

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Accumulate(Vector3 other)
        {
            X += other.X;
            Y += other.Y;
            Z += other.Z;
        }

        public double DotProduct(Vector3 other)
        {
            return (X * other.X) + (Y * other.Y) + (Z * other.Z);
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

        public double AngleWith(Vector3 other)
        {
            return Math.Acos(DotProduct(other) / (Magnitude * other.Magnitude));
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            var vector3 = new Vector3();
            vector3.Accumulate(a);
            vector3.Accumulate(b);
            return vector3;
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            var vector3 = new Vector3(a);
            vector3.X -= b.X;
            vector3.Y -= b.Y;
            vector3.Z -= b.Z;
            return vector3;
        }

        public Vector3 Inverse()
        {
            return new Vector3(-X, -Y, -Z);
        }

        public Vector3 UnitVector()
        {
            var m = Magnitude;
            return new Vector3(X / m, Y / m, Z / m);
        }

        public Vector3 WithScale(double d)
        {
            X = X*d;
            Y = Y*d;
            Z = Z*d;
            return this;
        }
    }
}
