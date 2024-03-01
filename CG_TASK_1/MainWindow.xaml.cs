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

namespace CG_TASK_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage originalBitmap;
        private BitmapImage filteredBitmap;
        private Bitmap originalImage;
        private Bitmap filteredImage;
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

        private void FilterWindow_FilterApplied(object sender, FilterAppliedEventArgs e)
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

        private Bitmap ApplyFilter(Bitmap image, int filterIndex)
        {
            switch (filterIndex)
            {
                case 0:
                    return image;
                case 1: 
                    return ApplyInversion(image);
                case 2: 
                    return ApplyBrightnessCorrection(image);
                case 3: 
                    return ApplyContrastEnhancement(image);
                case 4: 
                    return ApplyGammaCorrection(image);
                case 5: 
                    return ApplyBlur(image);
                case 6: 
                    return ApplyGaussianBlur(image);
                case 7: 
                    return ApplySharpen(image);
                case 8:
                    return ApplyEdgeDetection(image);
                case 9:
                    return ApplyEmboss(image);
                default:
                    return image;
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

        private Bitmap ApplyInversion(Bitmap image)
        {
            Bitmap invertedImage = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    System.Drawing.Color pixelColor = image.GetPixel(x, y);
                    System.Drawing.Color invertedColor = System.Drawing.Color.FromArgb(
                        255 - pixelColor.R,
                        255 - pixelColor.G,
                        255 - pixelColor.B
                    );
                    invertedImage.SetPixel(x, y, invertedColor);
                }
            }

            return invertedImage;
        }

        private Bitmap ApplyBrightnessCorrection(Bitmap image)
        {
            Bitmap correctedImage = new Bitmap(image.Width, image.Height);

            double brightness = 10;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    System.Drawing.Color pixelColor = image.GetPixel(x, y);

                    int newRed = Clamp((int)(pixelColor.R + brightness));
                    int newGreen = Clamp((int)(pixelColor.G + brightness));
                    int newBlue = Clamp((int)(pixelColor.B + brightness));

                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(pixelColor.A, newRed, newGreen, newBlue);
                    correctedImage.SetPixel(x, y, newColor);
                }
            }

            return correctedImage;
        }

        private int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }

        private Bitmap ApplyContrastEnhancement(Bitmap image)
        {
            Bitmap correctedImage = new Bitmap(image.Width, image.Height);

            double contrast = 10;

            double factor = (100.0f + contrast) / 100.0f;
            factor *= factor;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    System.Drawing.Color pixelColor = image.GetPixel(x, y);

                    double red = (((pixelColor.R / 255.0f) - 0.5f) * factor + 0.5f) * 255.0f;
                    double green = (((pixelColor.G / 255.0f) - 0.5f) * factor + 0.5f) * 255.0f;
                    double blue = (((pixelColor.B / 255.0f) - 0.5f) * factor + 0.5f) * 255.0f;

                    int newRed = Clamp((int)red);
                    int newGreen = Clamp((int)green);
                    int newBlue = Clamp((int)blue);

                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(pixelColor.A, newRed, newGreen, newBlue);
                    correctedImage.SetPixel(x, y, newColor);
                }
            }

            return correctedImage;
        }

        private Bitmap ApplyGammaCorrection(Bitmap image)
        {
            Bitmap correctedImage = new Bitmap(image.Width, image.Height);

            double gamma = 1.2;

            byte[] gammaCorrection = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                gammaCorrection[i] = (byte)Math.Min(255, (int)(255.0 * Math.Pow(i / 255.0, gamma) + 0.5));
            }

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    System.Drawing.Color pixelColor = image.GetPixel(x, y);
                    int newRed = gammaCorrection[pixelColor.R];
                    int newGreen = gammaCorrection[pixelColor.G];
                    int newBlue = gammaCorrection[pixelColor.B];

                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(pixelColor.A, newRed, newGreen, newBlue);
                    correctedImage.SetPixel(x, y, newColor);
                }
            }

            return correctedImage;
        }

        private Bitmap ApplyBlur(Bitmap image)
        {
            int[,] kernel = {
                { 1, 1, 1 },
                { 1, 1, 1 },
                { 1, 1, 1 }
            };

            return ApplyConvolution(image, kernel);
        }

        private Bitmap ApplyGaussianBlur(Bitmap image)
        {
            int[,] kernel = {
                { 0, 1, 0 },
                { 1, 4, 1 },
                { 0, 1, 0 }
            };

            return ApplyConvolution(image, kernel);
        }

        private Bitmap ApplySharpen(Bitmap image)
        {
            int[,] kernel = {
                { 0, -1, 0 },
                { -1, 5, -1 },
                { 0, -1, 0 }
            };

            return ApplyConvolution(image, kernel);
        }

        private Bitmap ApplyEdgeDetection(Bitmap image)
        {
            int[,] kernel = {
                { -1, -1, -1 },
                { -1, 8, -1 },
                { -1, -1, -1 }
            };

            return ApplyConvolution(image, kernel);
        }

        private Bitmap ApplyEmboss(Bitmap image)
        {
            int[,] kernel = {
                { -2, -1, 0 },
                { -1, 1, 1 },
                { 0, 1, 2 }
            };

            return ApplyConvolution(image, kernel);
        }

        private Bitmap ApplyConvolution(Bitmap image, int[,] kernel)
        {
            Bitmap filteredImage = new Bitmap(image.Width, image.Height);
            int kernelSize = kernel.GetLength(0);
            int radius = kernelSize / 2;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    int red = 0, green = 0, blue = 0;
                    int totalWeight = 0;

                    for (int i = 0; i < kernelSize; i++)
                    {
                        for (int j = 0; j < kernelSize; j++)
                        {
                            int xOffset = x - radius + j;
                            int yOffset = y - radius + i;

                            int pixelX = Clamp(xOffset, 0, image.Width - 1);
                            int pixelY = Clamp(yOffset, 0, image.Height - 1);

                            System.Drawing.Color pixelColor = image.GetPixel(pixelX, pixelY);

                            red += pixelColor.R * kernel[i, j];
                            green += pixelColor.G * kernel[i, j];
                            blue += pixelColor.B * kernel[i, j];
                            totalWeight += kernel[i, j];
                        }
                    }

                    if (totalWeight != 0)
                    {
                        red /= totalWeight;
                        green /= totalWeight;
                        blue /= totalWeight;
                    }

                    red = Math.Min(255, Math.Max(0, red));
                    green = Math.Min(255, Math.Max(0, green));
                    blue = Math.Min(255, Math.Max(0, blue));

                    filteredImage.SetPixel(x, y, System.Drawing.Color.FromArgb(red, green, blue));
                }
            }

            return filteredImage;
        }

        private int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
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
    }
}
