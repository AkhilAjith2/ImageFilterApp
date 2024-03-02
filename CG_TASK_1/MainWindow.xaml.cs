using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Media3D;

namespace CG_TASK_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static BitmapImage originalBitmap;
        public static BitmapImage filteredBitmap;
        public static Bitmap originalImage;
        public static Bitmap filteredImage;

        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.avif";
            if (openFileDialog.ShowDialog() == true)
            {
                originalImage = new Bitmap(openFileDialog.FileName);
                filteredImage = originalImage;
                originalBitmap = ConvertBitmapToBitmapImage(originalImage);
                OriginalImage.Source = originalBitmap;
                FilteredImage.Source = originalBitmap;
            }
        }
        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (FilteredImage.Source != null)
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "PNG Image|*.png";
                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)FilteredImage.Source));
                        encoder.Save(fileStream);
                    }
                    MessageBox.Show("Image saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please apply a filter to the image first.");
            }
        }

        private Stack<Bitmap> filterStack = new Stack<Bitmap>();

        private void OpenFilterWindow_Click(object sender, RoutedEventArgs e)
        {
            FilterWindow filterWindow = new FilterWindow();
            filterWindow.Owner = this;
            filterWindow.FilterApplied += FilterWindow_FilterApplied;
            filterWindow.Show();
        }

        public void FilterWindow_FilterApplied(object sender, FilterAppliedEventArgs e)
        {
            if (originalBitmap != null)
            {
                Bitmap filteredImageCopy = ApplyFilter(filteredImage, e.SelectedFilterIndex);
                filteredBitmap = ConvertBitmapToBitmapImage(filteredImageCopy);
                FilteredImage.Source = filteredBitmap;
                filterStack.Push(filteredImageCopy);
                filteredImage = new Bitmap(filteredImageCopy);
            }
            else
            {
                MessageBox.Show("Please load an image first.");
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (filterStack.Count > 0)
            {
                filterStack.Pop();

                if (filterStack.Count > 0)
                {
                    BitmapImage previousBitmap = ConvertBitmapToBitmapImage(filterStack.Peek());
                    FilteredImage.Source = previousBitmap;
                    filteredImage = new Bitmap(filterStack.Peek());
                }
                else
                {
                    FilteredImage.Source = originalBitmap;
                    filteredBitmap = originalBitmap;
                    filteredImage = originalImage;
                }
            }
            else
            {
                MessageBox.Show("No changes to undo.");
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            FilteredImage.Source = originalBitmap;
            filteredBitmap = originalBitmap;
            filteredImage = originalImage;
            filterStack.Clear();
        }

        public Bitmap ApplyFilter(Bitmap image, int filterIndex)
        {
            switch (filterIndex)
            {
                case 0:
                    return image;
                case 1:
                    return Filters.ApplyInversion(image);
                case 2:
                    return Filters.ApplyBrightnessCorrection(image);
                case 3:
                    return Filters.ApplyContrastEnhancement(image);
                case 4:
                    return Filters.ApplyGammaCorrection(image);
                case 5:
                    return Filters.ApplyBlur(image);
                case 6:
                    return Filters.ApplyGaussianBlur(image);
                case 7:
                    return Filters.ApplySharpen(image);
                case 8:
                    return Filters.ApplyEdgeDetection(image);
                case 9:
                    return Filters.ApplyEmboss(image);
                default:
                    return image;
            }
        }

        public static Bitmap ApplyConvolution(Bitmap image, int[,] kernel)
        {
            BitmapSource bitmapSource = ConvertBitmapToBitmapSource(image);
            BitmapSource filteredBitmapSource = ApplyConvolution(bitmapSource, kernel);
            filteredImage = ConvertBitmapSourceToBitmap(filteredBitmapSource);
            return filteredImage;
        }

        public static BitmapSource ApplyConvolution(BitmapSource original, int[,] kernel)
        {
            int width = original.PixelWidth;
            int height = original.PixelHeight;
            int totalWeight = 0;

            WriteableBitmap resultBitmap = new WriteableBitmap(original);
            Int32Rect rect = new Int32Rect(0, 0, width, height);
            byte[] pixels = new byte[width * height * 4];

            original.CopyPixels(rect, pixels, width * 4, 0);

            resultBitmap.Lock();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int[] colorSum = { 0, 0, 0 };

                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            int offsetX = Math.Max(0, Math.Min(width - 1, x + kx));
                            int offsetY = Math.Max(0, Math.Min(height - 1, y + ky));

                            int index = (offsetY * width + offsetX) * 4;
                            colorSum[0] += pixels[index] * kernel[ky + 1, kx + 1];
                            colorSum[1] += pixels[index + 1] * kernel[ky + 1, kx + 1];
                            colorSum[2] += pixels[index + 2] * kernel[ky + 1, kx + 1];
                            totalWeight += kernel[ky + 1, kx + 1];
                        }
                    }

                    if (totalWeight > 0)
                    {
                        colorSum[0] /= totalWeight;
                        colorSum[1] /= totalWeight;
                        colorSum[2] /= totalWeight;
                    }

                    byte red = (byte)Math.Min(255, Math.Max(0, colorSum[0]));
                    byte green = (byte)Math.Min(255, Math.Max(0, colorSum[1]));
                    byte blue = (byte)Math.Min(255, Math.Max(0, colorSum[2]));

                    int resultIndex = (y * width + x) * 4;
                    resultBitmap.WritePixels(new Int32Rect(x, y, 1, 1), new byte[] { red, green, blue, 255 }, 4, 0);

                    totalWeight = 0;
                }
            }

            resultBitmap.Unlock();

            return resultBitmap;
        }

        public static BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public static Bitmap ConvertBitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return new Bitmap(bitmap);
        }
    }
}
