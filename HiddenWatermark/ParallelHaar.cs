using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using Accord.Math.Wavelets;

namespace HiddenWatermark
{
    public class ParallelHaar : IWavelet
    {
        private const double w0 = 0.5;
        private const double w1 = -0.5;
        private const double s0 = 0.5;
        private const double s1 = 0.5;

        private int levels;

        public ParallelHaar(int levels)
        {
            this.levels = levels;
        }

        public void Forward(double[] data)
        {
            FWT(data);
        }

        public void Backward(double[] data)
        {
            IWT(data);
        }

        public void Forward(double[,] data)
        {
            FWT(data, levels);
        }

        public void Backward(double[,] data)
        {
            IWT(data, levels);
        }

        public static void FWT(double[] data)
        {
            double[] temp = new double[data.Length];

            int h = data.Length >> 1;
            for (int i = 0; i < h; i++)
            {
                int k = (i << 1);
                temp[i] = data[k] * s0 + data[k + 1] * s1;
                temp[i + h] = data[k] * w0 + data[k + 1] * w1;
            }

            for (int i = 0; i < data.Length; i++)
                data[i] = temp[i];
        }

        public static void IWT(double[] data)
        {
            double[] temp = new double[data.Length];

            int h = data.Length >> 1;
            for (int i = 0; i < h; i++)
            {
                int k = (i << 1);
                temp[k] = (data[i] * s0 + data[i + h] * w0) / w0;
                temp[k + 1] = (data[i] * s1 + data[i + h] * w1) / s0;
            }

            for (int i = 0; i < data.Length; i++)
                data[i] = temp[i];
        }

        public static void FWT(double[,] data, int iterations)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);

            for (int k = 0; k < iterations; k++)
            {
                int lev = 1 << k;

                int levCols = cols / lev;
                int levRows = rows / lev;

                Parallel.For(0, levRows, i =>
                {
                    var row = data.GetRow(i);
                    FWT(row);
                    data.SetRow(i, row);
                });

                Parallel.For(0, levCols, j =>
                {
                    var col = data.GetColumn(j);
                    FWT(col);
                    data.SetColumn(j, col);
                });
            }
        }

        public static void IWT(double[,] data, int iterations)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);

            for (int k = iterations - 1; k >= 0; k--)
            {
                int lev = 1 << k;

                int levCols = cols / lev;
                int levRows = rows / lev;

                Parallel.For(0, levCols, j =>
                {
                    var col = data.GetColumn(j);
                    IWT(col);
                    data.SetColumn(j, col);
                });

                Parallel.For(0, levRows, i =>
                {
                    var row = data.GetRow(i);
                    IWT(row);
                    data.SetRow(i, row);
                });
            }
        }
    }
}