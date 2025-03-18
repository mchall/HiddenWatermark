using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace HiddenWatermark
{
    internal class ImageHelpers
    {
        private delegate double ColorConversion(double red, double green, double blue);

        public ImageHelpers()
        {
        }

        public RgbData ReadPixels(byte[] fileBytes)
        {
            var image = CreateImage(fileBytes);
            return ReadPixels(image);
        }

        public byte[] MergeWatermarkPixels(byte[] fileBytes, byte[] watermarkBytes)
        {
            var image = CreateImage(fileBytes);

            if (image.Format != PixelFormats.Bgr32)
                image = ToBgr32(image);

            var pixelSize = image.Format.BitsPerPixel / 8;
            var width = image.PixelWidth;
            var height = image.PixelHeight;
            var pixelFormat = image.Format;

            var wmPixels = ScaleWatermark(watermarkBytes, width, height);

            byte[] pixels = new byte[height * width * pixelSize];
            image.CopyPixels(pixels, width * pixelSize, 0);

            Parallel.For(0, height, h =>
            {
                var hPos = h * width * pixelSize;
                for (int w = 0; w < width * pixelSize; w += pixelSize)
                {
                    var i = hPos + w;

                    pixels[i] = ToByte(pixels[i] + 128 - wmPixels[i]);
                    pixels[i + 1] = ToByte(pixels[i + 1] + 128 - wmPixels[i + 1]);
                    //pixels[i + 2] = ToByte(pixels[i + 2] + 128 - wmPixels[i + 2]);
                }
            });

            using (var encoderMemoryStream = new MemoryStream())
            {
                var bitmap = new WriteableBitmap(image.PixelWidth, image.PixelHeight, image.DpiX, image.DpiY, image.Format, image.Palette); 
                bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * pixelSize, 0);

                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(encoderMemoryStream);

                return encoderMemoryStream.ToArray();
            }
        }

        private BitmapSource ToBgr32(BitmapSource bitmap)
        {
            FormatConvertedBitmap newFormattedBitmapSource = new FormatConvertedBitmap();

            newFormattedBitmapSource.BeginInit();
            newFormattedBitmapSource.Source = bitmap;
            newFormattedBitmapSource.DestinationFormat = PixelFormats.Bgr32;
            newFormattedBitmapSource.EndInit();

            return newFormattedBitmapSource;
        }

        private byte[] ScaleWatermark(byte[] watermarkBytes, int width, int height)
        {
            var image = CreateImage(watermarkBytes, width, height);
            var wmPixelSize = image.Format.BitsPerPixel / 8;

            var wmPixels = new byte[height * width * wmPixelSize];
            image.CopyPixels(new Int32Rect(0, 0, width, height), wmPixels, width * wmPixelSize, 0);
            return wmPixels;
        }

        public byte[] SavePixels(RgbData data)
        {
            var width = data.Width;
            var height = data.Height;
            var pixelSize = PixelFormats.Bgr32.BitsPerPixel / 8;

            byte[] pixels = new byte[width * pixelSize * height];
            Parallel.For(0, height, h =>
            {
                var hPos = h * width * pixelSize;
                for (int w = 0; w < width; w++)
                {
                    var i = hPos + (w * pixelSize);

                    pixels[i] = ToByte(data.B[w, h]);
                    pixels[i + 1] = ToByte(data.G[w, h]);
                    pixels[i + 2] = ToByte(data.R[w, h]);
                }
            });

            var frame = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, BitmapPalettes.WebPalette);
            frame.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * pixelSize, 0);

            using (var encoderMemoryStream = new MemoryStream())
            {
                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(frame));
                encoder.Save(encoderMemoryStream);

                return encoderMemoryStream.ToArray();
            }
        }

        public double[,] ExtractWatermarkData(byte[] bytes, int width, int height)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new ArgumentNullException("binary");
            }

            using (var decoderMemoryStream = new MemoryStream(bytes))
            {
                var decoder = BitmapDecoder.Create(decoderMemoryStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                var frame = decoder.Frames[0];

                var newWidth = (int)Math.Round((frame.PixelWidth / (double)(frame.PixelWidth)) * width);
                var newHeight = (int)Math.Round((frame.PixelHeight / (double)(frame.PixelHeight)) * height);

                var image = CreateImage(bytes, newWidth, newHeight);
                return ReadPixels(image, ColorSpaceConversion.RgbToU);
            }
        }

        private static BitmapSource CreateImage(byte[] bytes, int decodePixelWidth = 0, int decodePixelHeight = 0)
        {
            if (bytes == null) return null;

            BitmapImage result = new BitmapImage();
            result.BeginInit();

            if (decodePixelWidth > 0)
                result.DecodePixelWidth = decodePixelWidth;

            if (decodePixelHeight > 0)
                result.DecodePixelHeight = decodePixelHeight;

            result.StreamSource = new MemoryStream(bytes);
            result.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            result.CacheOption = BitmapCacheOption.Default;
            result.EndInit();

            return result;
        }

        private RgbData ReadPixels(BitmapSource source)
        {
            var width = source.PixelWidth;
            var height = source.PixelHeight;
            var pixelSize = source.Format.BitsPerPixel / 8;

            byte[] pixels = new byte[height * width * pixelSize];
            source.CopyPixels(pixels, width * pixelSize, 0);

            double[,] R = new double[width, height];
            double[,] G = new double[width, height];
            double[,] B = new double[width, height];

            Parallel.For(0, height, h =>
            {
                var hPos = h * width * pixelSize;
                for (int w = 0; w < width; w++)
                {
                    var i = hPos + (w * pixelSize);

                    var gray = pixelSize == 1 ? pixels[i] : 0;
                    B[w, h] = pixelSize >= 3 ? pixels[i] : gray;
                    G[w, h] = pixelSize >= 3 ? pixels[i + 1] : gray;
                    R[w, h] = pixelSize >= 3 ? pixels[i + 2] : gray;
                }
            });

            return new RgbData(R, G, B);
        }

        private double[,] ReadPixels(BitmapSource source, ColorConversion convert)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            var pixelSize = source.Format.BitsPerPixel / 8;

            byte[] pixels = new byte[height * width * pixelSize];
            source.CopyPixels(pixels, width * pixelSize, 0);

            double[,] data = new double[width, height];
            Parallel.For(0, height, h =>
            {
                var hPos = h * width * pixelSize;
                for (int w = 0; w < width; w++)
                {
                    var i = hPos + (w * pixelSize);

                    var gray = pixelSize == 1 ? pixels[i] : 0;

                    var blue = pixelSize >= 3 ? pixels[i] : gray;
                    var green = pixelSize >= 3 ? pixels[i + 1] : gray;
                    //var red = pixelSize >= 3 ? pixels[i + 2] : gray; //U doesn't use Red

                    data[w, h] = convert(255, green, blue);
                }
            });

            return data;
        }

        private byte ToByte(double input)
        {
            if (input < 0)
                return 0;
            if (input > 255)
                return 255;
            return (byte)input;
        }
    }
}