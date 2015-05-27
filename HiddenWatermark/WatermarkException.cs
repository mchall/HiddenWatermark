using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenWatermark
{
    public class WatermarkException : Exception
    {
        public WatermarkException(string message)
            : base(message)
        { }
    }
}