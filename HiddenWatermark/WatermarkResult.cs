using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenWatermark
{
    public class WatermarkResult
    {
        public byte[] WatermarkedImage { get; internal set; }
        public double Similarity { get; private set; }
        public byte[] RecoveredWatermark { get; private set; }

        public WatermarkResult()
        { }

        public WatermarkResult(double similarity, byte[] wmBytes)
        {
            Similarity = similarity;
            RecoveredWatermark = wmBytes;
        }
    }
}