using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPGE
{
    public class Vec2d
    {
        public double X { get; set; }

        public double Y { get; set; }

        public static Vec2d XY(double x, double y) { return new Vec2d() { X = x, Y = y }; }
    }
}
