using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Physics.Engine;

namespace Physics.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var v1 = new Vector3(3, 4, 0);
            var v2 = new Vector3(0, 4, 0);

            //System.Console.WriteLine(v1.DotProduct(v2));

            System.Console.WriteLine(v1.AngleWith(v2).ToDegrees());

            if (Debugger.IsAttached)
            {
                System.Console.WriteLine("Press any key to exit...");
                System.Console.ReadKey();
            }
        }
    }
}
