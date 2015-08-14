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
            var markedImage = Watermark.Default.EmbedWatermark(GetImageBytes());

            Assert.IsTrue(Watermark.Default.RetrieveWatermark(markedImage).WatermarkDetected);
        }

        [TestMethod]
        public void Watermark_Retrieve_False()
        {
            Assert.IsFalse(Watermark.Default.RetrieveWatermark(GetImageBytes()).WatermarkDetected);
        }

        [TestMethod]
        public void Watermark_RetrieveAndEmbed()
        {
            var markedImage = Watermark.Default.EmbedWatermark(GetImageBytes());

            var result = Watermark.Default.RetrieveAndEmbedWatermark(markedImage);

            Assert.IsTrue(result.WatermarkDetected);
            Assert.IsNotNull(result.WatermarkedImage);
        }

        [TestMethod]
        public void Watermark_RetrieveAndEmbed_False()
        {
            var result = Watermark.Default.RetrieveAndEmbedWatermark(GetImageBytes());

            Assert.IsFalse(result.WatermarkDetected);
            Assert.IsNotNull(result.WatermarkedImage);
        }

        [TestMethod]
        public void Watermark_Cache_Test()
        {
            var imgBytes = GetImageBytes();
            Random rand = new Random();
            for (int i = 0; i < 1000; i++)
            {
                int w = rand.Next(20);
                int h = 100;

                var wm = ScaledWatermarkCache.TryGetScaledWatermark(w, h);
                if (wm == null)
                    ScaledWatermarkCache.AddScaledWatermark(w, h, imgBytes);
            }

            Assert.IsTrue(ScaledWatermarkCache._cache.Count == ScaledWatermarkCache.CacheSize);
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