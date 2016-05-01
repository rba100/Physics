using System;

namespace Physics.Engine
{
    public static class ExtentionMethods
    {
        public static double ToDegrees(this double radians)
        {
            return (radians / Math.PI) * 180;
        }
    }
}