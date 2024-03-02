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

namespace CG_TASK_1
{
    internal class Filters
    {
        private int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
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
            BitmapSource bitmapSource = ConvertBitmapToBitmapSource(image);
            BitmapSource filteredBitmapSource = ApplyConvolution(bitmapSource, kernel);
            filteredImage = ConvertBitmapSourceToBitmap(filteredBitmapSource);
            return filteredImage;
        }

        private BitmapSource ApplyConvolution(BitmapSource original, int[,] kernel)
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
                            // Handle pixels at the edge by mirroring
                            int offsetX = Math.Max(0, Math.Min(width - 1, x + kx));
                            int offsetY = Math.Max(0, Math.Min(height - 1, y + ky));

                            int index = (offsetY * width + offsetX) * 4;
                            colorSum[0] += pixels[index] * kernel[ky + 1, kx + 1];
                            colorSum[1] += pixels[index + 1] * kernel[ky + 1, kx + 1];
                            colorSum[2] += pixels[index + 2] * kernel[ky + 1, kx + 1];
                            totalWeight += kernel[ky + 1, kx + 1];
                        }
                    }

                    // Normalize RGB values
                    if (totalWeight > 0)
                    {
                        colorSum[0] /= totalWeight;
                        colorSum[1] /= totalWeight;
                        colorSum[2] /= totalWeight;
                    }

                    // Clip RGB values to [0, 255] range
                    byte red = (byte)Math.Min(255, Math.Max(0, colorSum[0]));
                    byte green = (byte)Math.Min(255, Math.Max(0, colorSum[1]));
                    byte blue = (byte)Math.Min(255, Math.Max(0, colorSum[2]));

                    int resultIndex = (y * width + x) * 4;
                    resultBitmap.WritePixels(new Int32Rect(x, y, 1, 1), new byte[] { red, green, blue, 255 }, 4, 0);

                    // Reset total weight for the next pixel
                    totalWeight = 0;
                }
            }

            resultBitmap.Unlock();

            return resultBitmap;
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

        private BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
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

        private Bitmap ConvertBitmapSourceToBitmap(BitmapSource bitmapSource)
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
