using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenWatermark
{
    internal static class ColorSpaceConversion
    {
        public static double RgbToY(double red, double green, double blue)
        {
            return 0.299 * red + 0.587 * green + 0.114 * blue;
        }

        public static double RgbToU(double red, double green, double blue)
        {
            return -0.147 * red - 0.289 * green + 0.436 * blue;
        }

        public static double RgbToV(double red, double green, double blue)
        {
            return 0.615 * red - 0.515 * green - 0.100 * blue;
        }

        public static double YuvToR(double y, double u, double v)
        {
            return y + 1.140 * v;
        }

        public static double YuvToG(double y, double u, double v)
        {
            return y - 0.395 * u - 0.581 * v;
        }

        public static double YuvToB(double y, double u, double v)
        {
            return y + 2.032 * u;
        }
    }
}