using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenWatermark
{
    internal class RgbData
    {
        public double[,] R { get; private set; }
        public double[,] G { get; private set; }
        public double[,] B { get; private set; }

        public int Width
        {
            get { return R.GetUpperBound(0) + 1; }
        }

        public int Height
        {
            get { return R.GetUpperBound(1) + 1; }
        }

        public RgbData(double[,] gray)
        {
            R = gray;
            G = gray;
            B = gray;
        }

        public RgbData(double[,] r, double[,] g, double[,] b)
        {
            R = r;
            G = g;
            B = b;
        }
    }
}