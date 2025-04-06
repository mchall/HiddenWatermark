﻿using System;
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
        /// Creates a new watermarking class.
        /// </summary>
        public Watermark()
            : this(DefaultWatermark)
        { }

        /// <summary>
        /// Creates a new watermarking class.
        /// </summary>
        /// <param name="watermarkBytes">32x32 watermark image</param>
        public Watermark(byte[] watermarkBytes)
        {
            _imageHelper = new ImageHelpers();

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
        {;
            var yuv = new YuvData(DiffWidth, DiffHeight);

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
        /// <param name="strength">Strength of the embedded watermark. Higher strengths might be more visible in an image, but lower strength might be difficult to retrieve.</param>
        /// <returns>Retrieve results and embedded image bytes in 'WatermarkedImage' property</returns>
        public WatermarkResult RetrieveAndEmbedWatermark(byte[] imageBytes, double strength = 1)
        {
            WatermarkResult result = new WatermarkResult();
            byte[] watermarkedImage = null;
            Parallel.Invoke(() => result = RetrieveWatermark(imageBytes), () => watermarkedImage = EmbedWatermark(imageBytes, strength));
            result.WatermarkedImage = watermarkedImage;
            return result;
        }

        /// <summary>
        /// Embeds a watermark in an image
        /// </summary>
        /// <param name="imageBytes">Image bytes</param>
        /// <param name="strength">Strength of the embedded watermark. Higher strengths might be more visible in an image, but lower strength might be difficult to retrieve.</param>
        /// <returns>Image bytes of embedded watermark</returns>
        public byte[] EmbedWatermark(byte[] imageBytes, double strength = 1)
        {
            return _imageHelper.MergeWatermarkPixels(imageBytes, _watermarkDiff, strength);
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