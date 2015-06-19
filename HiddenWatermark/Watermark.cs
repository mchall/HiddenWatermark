using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Accord.Math;

namespace HiddenWatermark
{
    public class Watermark
    {
        private static Watermark _default;
        public static Watermark Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new Watermark();
                }
                return _default;
            }
        }

        private ImageHelpers _imageHelper;

        private RgbData _watermarkPixels;
        private byte[] _watermarkDiff;

        private const int WatermarkSize = 32;
        private const int DiffWidth = 512;
        private const int DiffHeight = 512;
        private const int BlockSize = 4;

        /// <summary>
        /// Turn clipping support on/off
        /// </summary>
        public bool ClipSupport
        {
            get { return _imageHelper.ClipSupport; }
            set { _imageHelper.ClipSupport = value; }
        }

        /// <summary>
        /// Creates a new watermarking class.
        /// <para>This class caches some watermark data, so it's more efficient to keep an instance rather than create a new one for every embed operation.</para> 
        /// <para>Defaults clipping sizes are 1024x768, 911x683, 832x624, 800x600, 744x558, 700x525, 640x480, 600x450, 568x426, 508x373, 480x360, 448x336, 400x300, 360x270, 333x250.</para> 
        /// </summary>
        /// <param name="clipSupport">Whether the algorithm should support clipping</param>
        /// <param name="clippingWidths">Custom clipping widths</param>
        /// <param name="clippingHeights">Custom clipping heights</param>
        public Watermark(bool clipSupport = false, IEnumerable<int> clippingWidths = null, IEnumerable<int> clippingHeights = null)
            : this(DefaultWatermark, clipSupport, clippingWidths, clippingHeights)
        { }

        /// <summary>
        /// Creates a new watermarking class.
        /// <para>This class caches some watermark data, so it's more efficient to keep an instance rather than create a new one for every embed operation.</para> 
        /// <para>Defaults clipping sizes are 1024x768, 911x683, 832x624, 800x600, 744x558, 700x525, 640x480, 600x450, 568x426, 508x373, 480x360, 448x336, 400x300, 360x270, 333x250.</para> 
        /// </summary>
        /// <param name="watermarkBytes">32x32 watermark image</param>
        /// <param name="clipSupport">Whether the algorithm should support clipping</param>
        /// <param name="clippingWidths">Custom clipping widths</param>
        /// <param name="clippingHeights">Custom clipping heights</param>
        public Watermark(byte[] watermarkBytes, bool clipSupport = false, IEnumerable<int> clippingWidths = null, IEnumerable<int> clippingHeights = null)
        {
            _imageHelper = new ImageHelpers(clipSupport, clippingWidths, clippingHeights);

            _watermarkPixels = _imageHelper.ReadPixels(watermarkBytes);
            if (_watermarkPixels.Height != WatermarkSize || _watermarkPixels.Width != WatermarkSize)
            {
                throw new WatermarkException("Watermark must be 32x32 image");
            }
            GenerateWatermarkDiff();
        }

        private static byte[] DefaultWatermark
        {
            get 
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("HiddenWatermark.watermark.jpg"))
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        private void GenerateWatermarkDiff()
        {
            double[,] gray = new double[DiffWidth, DiffHeight];
            for (int i = 0; i < DiffWidth; i++)
            {
                for (int j = 0; j < DiffHeight; j++)
                {
                    gray[i, j] = 0;
                }
            }

            var rgbData = new RgbData(gray);
            var yuv = rgbData.ToYuv();

            EmbedWatermark(yuv.U);

            var rgb = yuv.ToRgb();

            double[,] red = new double[DiffWidth, DiffHeight];
            double[,] green = new double[DiffWidth, DiffHeight];
            double[,] blue = new double[DiffWidth, DiffHeight];

            for (int i = 0; i < DiffWidth; i++)
            {
                for (int j = 0; j < DiffHeight; j++)
                {
                    red[i, j] = 128 - rgb.R[i, j];
                    green[i, j] = 128 - rgb.G[i, j];
                    blue[i, j] = 128 - rgb.B[i, j];
                }
            }

            _watermarkDiff = _imageHelper.SavePixels(new RgbData(red, green, blue));
        }

        /// <summary>
        /// Embeds a watermark in an image and checks if the image already has a watermark
        /// </summary>
        /// <param name="imageBytes">Image bytes</param>
        /// <returns>Retrieve results and embedded image bytes in 'WatermarkedImage' property</returns>
        public WatermarkResult RetrieveAndEmbedWatermark(byte[] imageBytes)
        {
            WatermarkResult result = new WatermarkResult();
            byte[] watermarkedImage = null;
            Parallel.Invoke(() => result = RetrieveWatermark(imageBytes), () => watermarkedImage = EmbedWatermark(imageBytes));
            result.WatermarkedImage = watermarkedImage;
            return result;
        }

        /// <summary>
        /// Embeds a watermark in an image
        /// </summary>
        /// <param name="imageBytes">Image bytes</param>
        /// <returns>Image bytes of embedded watermark</returns>
        public byte[] EmbedWatermark(byte[] imageBytes)
        {
            return _imageHelper.MergeWatermarkPixels(imageBytes, _watermarkDiff);
        }

        /// <summary>
        /// Retrieves a watermark from an image
        /// </summary>
        /// <param name="imageBytes">Image bytes</param>
        /// <returns>Watermark retrieval results</returns>
        public WatermarkResult RetrieveWatermark(byte[] imageBytes)
        {
            var data = _imageHelper.ExtractWatermarkData(imageBytes, DiffWidth, DiffHeight);
            return RetrieveWatermark(data);
        }

        private void EmbedWatermark(double[,] data)
        {
            double[,] watermarkData = new double[_watermarkPixels.Width, _watermarkPixels.Height];
            for (int x = 0; x < _watermarkPixels.Width; x++)
            {
                for (int y = 0; y < _watermarkPixels.Height; y++)
                {
                    watermarkData[x, y] = _watermarkPixels.R[x, y] > 125 ? 255 : 0;
                }
            }

            ParallelHaar.FWT(data, 2);
            var subband = LL2(data);

            Parallel.For(0, _watermarkPixels.Height, y =>
            {
                for (int x = 0; x < _watermarkPixels.Width; x++)
                {
                    var block = subband.Submatrix(x * BlockSize, x * BlockSize + BlockSize - 1, y * BlockSize, y * BlockSize + BlockSize - 1);

                    CosineTransform.DCT(block);

                    var midbandSum = Math.Max(2, Math.Abs(MidBand(block).Sum()));
                    var sigma = (watermarkData[x, y] > 125 ? 3 : -3);

                    block[1, 2] += midbandSum * sigma;
                    block[2, 0] += midbandSum * sigma;
                    block[2, 1] += midbandSum * sigma;
                    block[2, 2] += midbandSum * sigma;

                    CosineTransform.IDCT(block);

                    for (int i = 0; i < BlockSize; i++)
                    {
                        for (int j = 0; j < BlockSize; j++)
                        {
                            subband[x * BlockSize + i, y * BlockSize + j] = block[i, j];
                        }
                    }
                }
            });

            BackApplySubBand(data, subband);
            ParallelHaar.IWT(data, 2);
        }

        private WatermarkResult RetrieveWatermark(double[,] data)
        {
            var origWidth = data.GetUpperBound(0) + 1;
            var origHeight = data.GetUpperBound(1) + 1;

            if (DiffWidth - origWidth > 0 || DiffHeight - origHeight > 0)
            {
                var top = (DiffHeight - origHeight) / 2;
                var left = (DiffWidth - origWidth) / 2;
                var bottom = (DiffHeight - origHeight) - top;
                var right = (DiffWidth - origWidth) - left;
                data = data.Pad(left, top, right, bottom);
            }

            double[,] recoveredWatermarkData = new double[_watermarkPixels.Width, _watermarkPixels.Height];

            ParallelHaar.FWT(data, 2);
            var subband = LL2(data);

            var width = subband.GetUpperBound(0) + 1;
            var height = subband.GetUpperBound(1) + 1;

            Parallel.For(0, _watermarkPixels.Height, y =>
            {
                for (int x = 0; x < _watermarkPixels.Width; x++)
                {
                    if (x * BlockSize + BlockSize > width) return;
                    if (y * BlockSize + BlockSize > height) return;

                    var block = subband.Submatrix(x * BlockSize, x * BlockSize + BlockSize - 1, y * BlockSize, y * BlockSize + BlockSize - 1);

                    CosineTransform.DCT(block);
                    recoveredWatermarkData[x, y] = (MidBand(block).Sum() > 0) ? 255 : 0;
                }
            });

            double similiar = 0;
            double total = (width / 4) * (height / 4);

            for (int x = 0; x < _watermarkPixels.Width; x++)
            {
                for (int y = 0; y < _watermarkPixels.Height; y++)
                {
                    var oldValue = _watermarkPixels.R[x, y] > 125 ? 255 : 0;
                    if (recoveredWatermarkData[x, y] == oldValue)
                    {
                        similiar++;
                    }
                }
            }

            var similarity = Math.Round((similiar / total) * 100);

            var recoveredData = new RgbData(recoveredWatermarkData);
            var recoveredWatermarkBytes = _imageHelper.SavePixels(recoveredData);

            return new WatermarkResult(similarity, recoveredWatermarkBytes);
        }

        private IEnumerable<double> MidBand(double[,] block)
        {
            yield return block[1, 2];
            yield return block[2, 0];
            yield return block[2, 1];
            yield return block[2, 2];
        }

        private void BackApplySubBand(double[,] original, double[,] subBandData)
        {
            var width = original.GetUpperBound(0) + 1;
            var height = original.GetUpperBound(1) + 1;

            for (int x = 0; x < width / 4; x++)
            {
                for (int y = 0; y < height / 4; y++)
                {
                    original[x, y] = subBandData[x, y];
                }
            }
        }

        private double[,] LL2(double[,] dwtData)
        {
            var width = dwtData.GetUpperBound(0) + 1;
            var height = dwtData.GetUpperBound(1) + 1;

            return dwtData.Submatrix(0, width / 4 - 1, 0, height / 4 - 1);
        }
    }
}