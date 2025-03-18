using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenWatermark
{
    internal class YuvData
    {
        public double[,] Y { get; private set; }
        public double[,] U { get; private set; }
        public double[,] V { get; private set; }

        public int Width
        {
            get { return Y.GetUpperBound(0) + 1; }
        }

        public int Height
        {
            get { return Y.GetUpperBound(1) + 1; }
        }

        public YuvData(int width, int height)
        {
            Y = new double[width, height];
            U = new double[width, height];
            V = new double[width, height];
        }

        public RgbData ToRgb()
        {
            var width = Width;
            var height = Height;

            double[,] red = new double[width, height];
            double[,] green = new double[width, height];
            double[,] blue = new double[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    red[x, y] = ColorSpaceConversion.YuvToR(Y[x, y], U[x, y], V[x, y]);
                    green[x, y] = ColorSpaceConversion.YuvToG(Y[x, y], U[x, y], V[x, y]);
                    blue[x, y] = ColorSpaceConversion.YuvToB(Y[x, y], U[x, y], V[x, y]);
                }
            }

            return new RgbData(red, green, blue);
        }
    }
}