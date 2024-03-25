using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Numerics;
using System.Runtime.InteropServices;


namespace CG_TASK_1
{
    /// <summary>
    /// Interaction logic for KMeansWindow.xaml
    /// </summary>
    ///

    public class KMeansColorQuantization
    {
        public static BitmapSource ApplyKMeans(BitmapSource originalImage, int k, int maxIterations)
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(originalImage);

            List<Color> centroids = InitializeCentroids(writeableBitmap, k);

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                List<Color> newCentroids = UpdateCentroidsAndAssignPixels(writeableBitmap, centroids);

                if (CentroidsConverged(centroids, newCentroids))
                {
                    centroids = newCentroids;
                    break;
                }

                centroids = newCentroids;
            }

            ApplyFinalColors(writeableBitmap, centroids);

            return writeableBitmap;
        }

        private static List<Color> InitializeCentroids(WriteableBitmap bitmap, int k)
        {
            List<Color> centroids = new List<Color>();

            byte[] pixels = GetPixels(bitmap);
            Random rand = new Random();

            HashSet<int> chosenIndices = new HashSet<int>();

            for (int i = 0; i < k; i++)
            {
                int index = rand.Next(0, pixels.Length / 4);
                while (chosenIndices.Contains(index))
                {
                    index = rand.Next(0, pixels.Length / 4);
                }

                chosenIndices.Add(index);

                centroids.Add(Color.FromArgb(pixels[index * 4 + 3], pixels[index * 4 + 2], pixels[index * 4 + 1], pixels[index * 4]));
            }

            return centroids;
        }

        private static List<Color> UpdateCentroidsAndAssignPixels(WriteableBitmap bitmap, List<Color> centroids)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            byte[] pixels = GetPixels(bitmap);

            int[] totalR = new int[centroids.Count];
            int[] totalG = new int[centroids.Count];
            int[] totalB = new int[centroids.Count];
            int[] clusterCounts = new int[centroids.Count];

            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;

                    Color pixelColor = Color.FromArgb(pixels[index * 4 + 3], pixels[index * 4 + 2], pixels[index * 4 + 1], pixels[index * 4]);
                    int closestCentroidIndex = FindClosestCentroidIndex(pixelColor, centroids);

                    Interlocked.Add(ref totalR[closestCentroidIndex], pixelColor.R);
                    Interlocked.Add(ref totalG[closestCentroidIndex], pixelColor.G);
                    Interlocked.Add(ref totalB[closestCentroidIndex], pixelColor.B);
                    Interlocked.Increment(ref clusterCounts[closestCentroidIndex]);
                }
            });

            List<Color> newCentroids = new List<Color>();
            for (int i = 0; i < centroids.Count; i++)
            {
                byte newR = clusterCounts[i] == 0 ? (byte)0 : (byte)(totalR[i] / clusterCounts[i]);
                byte newG = clusterCounts[i] == 0 ? (byte)0 : (byte)(totalG[i] / clusterCounts[i]);
                byte newB = clusterCounts[i] == 0 ? (byte)0 : (byte)(totalB[i] / clusterCounts[i]);
                newCentroids.Add(Color.FromRgb(newR, newG, newB));
            }

            return newCentroids;
        }

        private static int FindClosestCentroidIndex(Color color, List<Color> centroids)
        {
            int closestIndex = 0;
            double minDistanceSquared = double.MaxValue;

            for (int i = 0; i < centroids.Count; i++)
            {
                double distanceSquared = ColorDistanceSquared(color, centroids[i]);
                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private static double ColorDistanceSquared(Color c1, Color c2)
        {
            int dr = c1.R - c2.R;
            int dg = c1.G - c2.G;
            int db = c1.B - c2.B;
            return dr * dr + dg * dg + db * db;
        }

        private static bool CentroidsConverged(List<Color> centroids, List<Color> newCentroids)
        {
            for (int i = 0; i < centroids.Count; i++)
            {
                if (centroids[i] != newCentroids[i])
                    return false;
            }
            return true;
        }

        private static void ApplyFinalColors(WriteableBitmap bitmap, List<Color> centroids)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            byte[] pixels = GetPixels(bitmap);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                Color pixelColor = Color.FromArgb(pixels[i + 3], pixels[i + 2], pixels[i + 1], pixels[i]);

                int closestCentroidIndex = FindClosestCentroidIndex(pixelColor, centroids);
                Color closestCentroid = centroids[closestCentroidIndex];

                pixels[i + 2] = closestCentroid.R;
                pixels[i + 1] = closestCentroid.G;
                pixels[i] = closestCentroid.B;
            }

            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
        }

        private static byte[] GetPixels(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            byte[] pixels = new byte[width * height * 4];
            bitmap.CopyPixels(pixels, width * 4, 0);

            return pixels;
        }
    }

    public partial class KMeansWindow : Window
    {
        public int K { get; private set; }
        public int MaxIterations { get; private set; }
        public KMeansWindow()
        {
            InitializeComponent();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(KTextBox.Text, out int k))
            {
                K = k;
                MaxIterations = 1000;
                DialogResult = true;
        }
            else
            {
                MessageBox.Show("Please enter valid integer values for K and Max Iterations.");
            }
}
    }
}
