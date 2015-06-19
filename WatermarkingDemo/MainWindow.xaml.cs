using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HiddenWatermark;
using Microsoft.Win32;

namespace WatermarkingDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _imageLocation;
        private string _watermarkImageLocation;
        private string _recoveredWatermarkLocation;

        private Watermark _watermark;

        public MainWindow()
        {
            InitializeComponent();

            _imageLocation = AppDomain.CurrentDomain.BaseDirectory + "original.jpg";
            _watermarkImageLocation = AppDomain.CurrentDomain.BaseDirectory + "embeddedwatermark.jpg";
            _recoveredWatermarkLocation = AppDomain.CurrentDomain.BaseDirectory + "recoveredwatermark.jpg";

            var fileBytes = File.ReadAllBytes(_imageLocation);
            RenderImageBytes(OriginalImage, fileBytes);

            _watermark = new Watermark(true);
        }

        private void BtnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image|*.jpg;*.png;*.gif;*.bmp";
            if (ofd.ShowDialog() == true)
            {
                _imageLocation = ofd.FileName;

                var fileBytes = File.ReadAllBytes(_imageLocation);
                RenderImageBytes(OriginalImage, fileBytes);
            }
        }

        private void BtnLoadWatermarkedImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image|*.jpg;*.png;*.gif;*.bmp";
            if (ofd.ShowDialog() == true)
            {
                _watermarkImageLocation = ofd.FileName;
                var fileBytes = File.ReadAllBytes(_watermarkImageLocation);
                RenderImageBytes(WatermarkedImage, fileBytes);
            }
        }

        private void BtnSaveWatermarkedImage_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image|*.jpg;*.png;*.gif;*.bmp";
            if (sfd.ShowDialog() == true)
            {
                File.Copy(_watermarkImageLocation, sfd.FileName);
            }
        }

        private void BtnEmbedWatermark_Click(object sender, RoutedEventArgs e)
        {
            var fileBytes = File.ReadAllBytes(_imageLocation);

            var sw = Stopwatch.StartNew();
            var embeddedBytes = _watermark.EmbedWatermark(fileBytes);
            //var embeddedBytes = _watermark.RetrieveAndEmbedWatermark(fileBytes).WatermarkedImage;
            sw.Stop();

            EmbedTime.Text = String.Format("{0}ms", sw.ElapsedMilliseconds);
            _watermarkImageLocation = AppDomain.CurrentDomain.BaseDirectory + "embeddedwatermark.jpg";

            File.WriteAllBytes(_watermarkImageLocation, embeddedBytes);
            RenderImageBytes(WatermarkedImage, embeddedBytes);
        }

        private void BtnRetrieveWatermark_Click(object sender, RoutedEventArgs e)
        {
            var fileBytes = File.ReadAllBytes(_watermarkImageLocation);

            var sw = Stopwatch.StartNew();
            var result = _watermark.RetrieveWatermark(fileBytes);
            sw.Stop();

            RetrieveTime.Text = String.Format("{0}ms", sw.ElapsedMilliseconds);
            SimilarityText.Text = String.Format("Similarity: {0}%", result.Similarity);

            if (result.WatermarkDetected)
            {
                SuccessImage.Visibility = Visibility.Visible;
                FailureImage.Visibility = Visibility.Collapsed;
            }
            else
            {
                SuccessImage.Visibility = Visibility.Collapsed;
                FailureImage.Visibility = Visibility.Visible;
            }

            File.WriteAllBytes(_recoveredWatermarkLocation, result.RecoveredWatermark);
            RenderImageBytes(RetrievedWatermark, result.RecoveredWatermark);
        }

        private void RenderImageBytes(System.Windows.Controls.Image control, byte[] bytes)
        {
            MemoryStream byteStream = new MemoryStream(bytes);
            BitmapImage imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = byteStream;
            imageSource.EndInit();

            control.Source = imageSource;
        }
    }
}