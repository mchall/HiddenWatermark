using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HiddenWatermark.Tests
{
    [TestClass]
    public class WatermarkTests
    {
        [TestMethod]
        public void Watermark_Retrieve()
        {
            Watermark watermark = new Watermark();
            var markedImage = watermark.EmbedWatermark(GetImageBytes());

            Assert.IsTrue(watermark.RetrieveWatermark(markedImage).WatermarkDetected);
        }

        [TestMethod]
        public void Watermark_Retrieve_False()
        {
            Watermark watermark = new Watermark();
            Assert.IsFalse(watermark.RetrieveWatermark(GetImageBytes()).WatermarkDetected);
        }

        [TestMethod]
        public void Watermark_RetrieveAndEmbed()
        {
            Watermark watermark = new Watermark();
            var markedImage = watermark.EmbedWatermark(GetImageBytes());

            var result = watermark.RetrieveAndEmbedWatermark(markedImage);

            Assert.IsTrue(result.WatermarkDetected);
            Assert.IsNotNull(result.WatermarkedImage);
        }

        [TestMethod]
        public void Watermark_RetrieveAndEmbed_False()
        {
            Watermark watermark = new Watermark();
            var result = watermark.RetrieveAndEmbedWatermark(GetImageBytes());

            Assert.IsFalse(result.WatermarkDetected);
            Assert.IsNotNull(result.WatermarkedImage);
        }

        private byte[] GetImageBytes()
        {
            return GetResourceBytes("HiddenWatermark.Tests.original.jpg");
        }

        private byte[] GetResourceBytes(string resourceName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}