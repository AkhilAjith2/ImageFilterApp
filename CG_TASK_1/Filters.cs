using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Input;

namespace CG_TASK_1
{
    public class Filter
    {
        public string Name { get; set; }
        public int[,] Kernel { get; set; }
        public System.Drawing.Point Anchor { get; set; }
        public int Divisor { get; set; }
        public int Offset { get; set; }
        public Filter(string name, int[,] kernel, System.Drawing.Point anchor, int divisor, int offset)
        {
            Name = name;
            Kernel = kernel;
            Anchor = anchor;
            Divisor = divisor;
            Offset = offset;
        }

        public Bitmap ApplyFilter(Bitmap image)
        {
            return Filters.ApplyConvolution(image, Kernel, Anchor, Divisor, Offset);
        }
    }

    public static class FilterManager
    {
        public static List<Filter> ConvolutionFilters { get; } = new List<Filter>();
        public static void AddPredefinedFilters()
        {
            ConvolutionFilters.Add(new Filter("Blur", new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } }, new System.Drawing.Point(1, 1), 9, 0));
            ConvolutionFilters.Add(new Filter("Gaussian Blur", new int[,] { { 0, 1, 0 }, { 1, 4, 1 }, { 0, 1, 0 } }, new System.Drawing.Point(1, 1), 8, 0));
            ConvolutionFilters.Add(new Filter("Sharpen", new int[,] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } }, new System.Drawing.Point(1, 1), 1, 0));
            ConvolutionFilters.Add(new Filter("Edge Detection", new int[,] { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } }, new System.Drawing.Point(1, 1), 0, 0));
            ConvolutionFilters.Add(new Filter("Emboss", new int[,] { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } }, new System.Drawing.Point(1, 1), 1, 0));
        }
    }

    internal class Filters
    {
        public static int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }

        public static Bitmap ApplyInversion(Bitmap image)
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

        public static Bitmap ApplyBrightnessCorrection(Bitmap image)
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

        public static Bitmap ApplyContrastEnhancement(Bitmap image)
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

        public static Bitmap ApplyGammaCorrection(Bitmap image)
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

        public static Bitmap ApplyBlur(Bitmap image)
        {
            Filter blurFilter = new Filter("Blur", new int[,] {
                { 1, 1, 1 },
                { 1, 1, 1 },
                { 1, 1, 1 }
            }, new System.Drawing.Point(1, 1), 9, 0);
            FilterManager.ConvolutionFilters.Add(FilterWindow.blurFilter);
            return blurFilter.ApplyFilter(image);
        }

        public static Bitmap ApplyGaussianBlur(Bitmap image)
        {
            Filter gaussianBlurFilter = new Filter("Gaussian Blur", new int[,] {
                { 0, 1, 0 },
                { 1, 4, 1 },
                { 0, 1, 0 }
            }, new System.Drawing.Point(1, 1), 8, 0);
            FilterManager.ConvolutionFilters.Add(FilterWindow.gaussianBlurFilter);
            return gaussianBlurFilter.ApplyFilter(image);
        }

        public static Bitmap ApplySharpen(Bitmap image)
        {
            Filter sharpenFilter = new Filter("Sharpen", new int[,] {
                { 0, -1, 0 },
                { -1, 5, -1 },
                { 0, -1, 0 }
            }, new System.Drawing.Point(1, 1), 1, 0);
            FilterManager.ConvolutionFilters.Add(FilterWindow.sharpenFilter);
            return sharpenFilter.ApplyFilter(image);
        }

        public static Bitmap ApplyEdgeDetection(Bitmap image)
        {
            Filter edgeDetectionFilter = new Filter("Edge Detection", new int[,] {
                { -1, -1, -1 },
                { -1, 8, -1 },
                { -1, -1, -1 }
            }, new System.Drawing.Point(1, 1), 0, 0);
            FilterManager.ConvolutionFilters.Add(FilterWindow.edgeDetectionFilter);
            return edgeDetectionFilter.ApplyFilter(image);
        }

        public static Bitmap ApplyEmboss(Bitmap image)
        {
            Filter embossFilter = new Filter("Emboss", new int[,] {
                { -2, -1, 0 },
                { -1, 1, 1 },
                { 0, 1, 2 }
            }, new System.Drawing.Point(1, 1), 1, 0);
            FilterManager.ConvolutionFilters.Add(FilterWindow.embossFilter);
            return embossFilter.ApplyFilter(image);
        }

        public static Bitmap ApplyFilter(Bitmap image, int filterIndex)
        {
            Filter filter = GetFilter(filterIndex);

            if (filterIndex <= 4)
            {
                switch (filterIndex)
                {
                    case 1:
                        return ApplyInversion(image);
                    case 2:
                        return ApplyBrightnessCorrection(image);
                    case 3:
                        return ApplyContrastEnhancement(image);
                    case 4:
                        return ApplyGammaCorrection(image);
                    default:
                        return image;
                }
            }
            else
            {
                if (filter == null)
                {
                    switch (filterIndex)
                    {
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
                        case 10:
                            return ApplyMedianFilter(image);
                        case 11:
                            return ConvertToGrayScale(image); 
                        default:
                            return image;
                    }
                }
                else
                {
                    return ApplyConvolution(image, filter.Kernel, filter.Anchor, filter.Divisor, filter.Offset);
                }
            }
        }

        private static Filter GetFilter(int filterIndex)
        {
            if (filterIndex >= FilterWindow.predefinedFilters.Count && filterIndex < FilterWindow.predefinedFilters.Count + KernelEditingWindow.savedFilters.Count)
            {
                return KernelEditingWindow.savedFilters[filterIndex - FilterWindow.predefinedFilters.Count];
            }
            else
            {
                return null;
            }
        }

        public static Bitmap ApplyConvolution(Bitmap image, int[,] kernel, System.Drawing.Point anchor, int divisor, int offset)
        {
            BitmapSource bitmapSource = ConvertBitmapToBitmapSource(image);
            BitmapSource filteredBitmapSource = ApplyConvolution(bitmapSource, kernel, anchor, divisor, offset);
            MainWindow.filteredImage = ConvertBitmapSourceToBitmap(filteredBitmapSource);
            return MainWindow.filteredImage;
        }

        public static BitmapSource ApplyConvolution(BitmapSource original, int[,] kernel, System.Drawing.Point anchor, int divisor, int offset)
        {
            int width = original.PixelWidth;
            int height = original.PixelHeight;

            WriteableBitmap resultBitmap = new WriteableBitmap(original);
            Int32Rect rectangle = new Int32Rect(0, 0, width, height);
            byte[] pixels = new byte[width * height * 4];
            byte[] resultPixels = new byte[width * height * 4];

            original.CopyPixels(rectangle, pixels, width * 4, 0);

            int kernelSizeX = kernel.GetLength(1);
            int kernelSizeY = kernel.GetLength(0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int[] colorSum = { 0, 0, 0 };

                    for (int ky = 0; ky < kernelSizeY; ky++)
                    {
                        for (int kx = 0; kx < kernelSizeX; kx++)
                        {
                            int offsetX = Math.Max(0, Math.Min(width - 1, x + kx - anchor.X));
                            int offsetY = Math.Max(0, Math.Min(height - 1, y + ky - anchor.Y));

                            int index = (offsetY * width + offsetX) * 4;
                            colorSum[0] += pixels[index] * kernel[ky, kx];
                            colorSum[1] += pixels[index + 1] * kernel[ky, kx];
                            colorSum[2] += pixels[index + 2] * kernel[ky, kx];
                        }
                    }

                    if (divisor != 0)
                    {
                        colorSum[0] = (colorSum[0] / divisor) + offset;
                        colorSum[1] = (colorSum[1] / divisor) + offset;
                        colorSum[2] = (colorSum[2] / divisor) + offset;
                    }

                    byte red = (byte)Math.Min(255, Math.Max(0, colorSum[0]));
                    byte green = (byte)Math.Min(255, Math.Max(0, colorSum[1]));
                    byte blue = (byte)Math.Min(255, Math.Max(0, colorSum[2]));

                    int resultIndex = (y * width + x) * 4;
                    resultPixels[resultIndex] = red;
                    resultPixels[resultIndex + 1] = green;
                    resultPixels[resultIndex + 2] = blue;
                    resultPixels[resultIndex + 3] = 255; 
                }
            }

            resultBitmap.WritePixels(rectangle, resultPixels, width * 4, 0); 

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

        public static Bitmap ApplyMedianFilter(Bitmap image)
        {
            Bitmap filteredImage = new Bitmap(image.Width, image.Height);

            for (int y = 1; y < image.Height - 1; y++)
            {
                for (int x = 1; x < image.Width - 1; x++)
                {
                    System.Drawing.Color[] neighborhood = new System.Drawing.Color[9];
                    int index = 0;
                    for (int j = y - 1; j <= y + 1; j++)
                    {
                        for (int i = x - 1; i <= x + 1; i++)
                        {
                            neighborhood[index++] = image.GetPixel(i, j);
                        }
                    }

                    Array.Sort(neighborhood, (c1, c2) => GetIntensity(c1).CompareTo(GetIntensity(c2)));

                    System.Drawing.Color medianColor = neighborhood[4];

                    filteredImage.SetPixel(x, y, medianColor);
                }
            }
            return filteredImage;
        }

        private static int GetIntensity(System.Drawing.Color color)
        {
            return (color.R + color.G + color.B) / 3;
        }

        public static Bitmap ApplyRandomDithering(Bitmap image, int k)
        {
            BitmapSource bitmapSource = ConvertBitmapToBitmapSource(image);
            bool isGreyscale = IsGrayscale(image);
            BitmapSource filteredBitmapSource = ApplyRandomDithering(bitmapSource, k,isGreyscale);
            MainWindow.filteredImage = ConvertBitmapSourceToBitmap(filteredBitmapSource);
            return MainWindow.filteredImage;
        }

        public static BitmapSource ApplyRandomDithering(BitmapSource originalImage, int k, bool isGreyscale)
        {
            int width = originalImage.PixelWidth;
            int height = originalImage.PixelHeight;

            byte[] pixels = new byte[height * width * 4];
            originalImage.CopyPixels(pixels, width * 4, 0);

            Random rand = new Random();
           
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double temp = rand.NextDouble();
                    int index = (y * width + x) * 4;

                    if (isGreyscale)
                    {
                        byte originalIntensity = (byte)((0.299 * pixels[index + 2]) + (0.587 * pixels[index + 1]) + (0.114 * pixels[index]));
                        int newIntensity = ApplyDithering(originalIntensity, temp, k);
                        byte newColor = (byte)newIntensity;

                        pixels[index] = newColor;
                        pixels[index + 1] = newColor;
                        pixels[index + 2] = newColor;
                        pixels[index + 3] = 255;
                    }
                    else
                    {
                        byte originalRed = pixels[index + 2];
                        byte originalGreen = pixels[index + 1];
                        byte originalBlue = pixels[index];

                        

                        byte newRed = (byte)ApplyDithering(originalRed, temp, k);
                        byte newGreen = (byte)ApplyDithering(originalGreen, temp, k);
                        byte newBlue = (byte)ApplyDithering(originalBlue, temp, k);

                        pixels[index + 2] = newRed;
                        pixels[index + 1] = newGreen;
                        pixels[index] = newBlue;
                    }
                }
            }

            WriteableBitmap ditheredImage = new WriteableBitmap(width, height, originalImage.DpiX, originalImage.DpiY, PixelFormats.Bgra32, null);
            ditheredImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

            return ditheredImage;
        }


        private static bool IsGrayscale(Bitmap image)
        {

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    System.Drawing.Color pixelColor = image.GetPixel(x, y);
                    if (pixelColor.R != pixelColor.G || pixelColor.R != pixelColor.B)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static int ApplyDithering(int intensity, double randDouble, int k)
        {
            int index = (int)Math.Floor(intensity * (k - 1) / 255.0);

            if (index < k - 1 && (intensity * (k - 1) / 255.0 - index) > randDouble)
            {
                index++;
            }

            int quantizedIntensity = index * 255 / (k - 1);
            return quantizedIntensity;
        }


        /*public static Bitmap ApplyRandomDithering(Bitmap originalImage, int k)
        {
            Bitmap ditheredImage = new Bitmap(originalImage.Width, originalImage.Height);
            Random rand = new Random();

            // Iterate through each pixel
            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    System.Drawing.Color originalColor = originalImage.GetPixel(x, y);

                    // Convert to grayscale intensity
                    int intensity = (int)(originalColor.R * 0.299 + originalColor.G * 0.587 + originalColor.B * 0.114);

                    // Apply dithering
                    int newIntensity = ApplyDithering(intensity, rand, k);

                    // Convert back to color
                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(newIntensity, newIntensity, newIntensity);

                    // Set pixel in the dithered image
                    ditheredImage.SetPixel(x, y, newColor);
                }
            }

            return ditheredImage;
        }

        private static int ApplyDithering(int intensity, Random rand, int k)
        {
            double noise = (rand.NextDouble() - 0.5) * (255.0 / k);
            int ditheredValue = intensity + (int)Math.Round(noise);

            // Clamp the value to the valid range [0, 255]
            if (ditheredValue < 0)
                ditheredValue = 0;
            else if (ditheredValue > 255)
                ditheredValue = 255;

            return ditheredValue;
        }*/

        public static Bitmap ConvertToGrayScale(Bitmap originalImage)
        {
            Bitmap grayScaleImage = new Bitmap(originalImage.Width, originalImage.Height);

            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    System.Drawing.Color originalColor = originalImage.GetPixel(x, y);
                    int intensity = (int)(0.299 * originalColor.R + 0.587 * originalColor.G + 0.114 * originalColor.B);
                    System.Drawing.Color grayColor = System.Drawing.Color.FromArgb(intensity, intensity, intensity);
                    grayScaleImage.SetPixel(x, y, grayColor);
                }
            }

            return grayScaleImage;
        }

        public static Bitmap ApplyDitheringToYCbCr(Bitmap image, int k)
        {
            BitmapSource bitmapSource = ConvertBitmapToBitmapSource(image);
            BitmapSource filteredBitmapSource = ApplyDitheringToYCbCr(bitmapSource, k);
            MainWindow.filteredImage = ConvertBitmapSourceToBitmap(filteredBitmapSource);
            return MainWindow.filteredImage;
        }

        public static BitmapSource ApplyDitheringToYCbCr(BitmapSource originalImage, int k)
        {
            int width = originalImage.PixelWidth;
            int height = originalImage.PixelHeight;

            byte[] pixels = new byte[height * width * 4];
            originalImage.CopyPixels(pixels, width * 4, 0);

            WriteableBitmap ditheredImage = new WriteableBitmap(width, height, originalImage.DpiX, originalImage.DpiY, PixelFormats.Bgra32, null);
            Random rand = new Random();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double temp = rand.NextDouble();
                    int index = (y * width + x) * 4;

                    double originalR = pixels[index + 2];
                    double originalG = pixels[index + 1];
                    double originalB = pixels[index];

                    double yPrime = 0.299 * originalR + 0.587 * originalG + 0.114 * originalB;
                    double cb = 128 - 0.168736 * originalR - 0.331264 * originalG + 0.5 * originalB;
                    double cr = 128 + 0.5 * originalR - 0.418688 * originalG - 0.081312 * originalB;

                    double newYPrime = ApplyDithering((int)Math.Round(yPrime), temp, k);
                    double newCb = ApplyDithering((int)Math.Round(cb), temp, k);
                    double newCr = ApplyDithering((int)Math.Round(cr), temp, k);

                    int newR = (int)(newYPrime + 1.402 * (newCr - 128));
                    int newG = (int)(newYPrime - 0.344136 * (newCb - 128) - 0.714136 * (newCr - 128));
                    int newB = (int)(newYPrime + 1.772 * (newCb - 128));

                    newR = Math.Max(0, Math.Min(255, newR));
                    newG = Math.Max(0, Math.Min(255, newG));
                    newB = Math.Max(0, Math.Min(255, newB));

                    pixels[index + 2] = (byte)newR;
                    pixels[index + 1] = (byte)newG;
                    pixels[index] = (byte)newB;
                    pixels[index + 3] = 255;
                }
            }

            ditheredImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

            return ditheredImage;
        }

    }
}
