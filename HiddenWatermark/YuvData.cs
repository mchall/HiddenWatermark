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

        public YuvData(RgbData rgb)
        {
            Y = new double[rgb.Width, rgb.Height];
            U = new double[rgb.Width, rgb.Height];
            V = new double[rgb.Width, rgb.Height];

            for (int i = 0; i < rgb.Width; i++)
            {
                for (int j = 0; j < rgb.Height; j++)
                {
                    Y[i, j] = ColorSpaceConversion.RgbToY(rgb.R[i, j], rgb.G[i, j], rgb.B[i, j]);
                    U[i, j] = ColorSpaceConversion.RgbToU(rgb.R[i, j], rgb.G[i, j], rgb.B[i, j]);
                    V[i, j] = ColorSpaceConversion.RgbToV(rgb.R[i, j], rgb.G[i, j], rgb.B[i, j]);
                }
            }
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