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

namespace CG_TASK_1
{
    /// <summary>
    /// Interaction logic for KMeansWindow.xaml
    /// </summary>
    ///

    public class KMeansColorQuantization
    {
        private static byte[] pixels;
        public static BitmapSource ApplyKMeans(BitmapSource originalImage, int k, int maxIterations)
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(originalImage);

            List<Color> centroids = InitializeCentroids(writeableBitmap, k);

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                /*Dictionary<Color, List<Color>> clusters = AssignPixelsToCentroids(writeableBitmap, centroids);
                List<Color> newCentroids = UpdateCentroids(clusters);*/

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

            Random rand = new Random();

            pixels = GetPixels(bitmap);

            for (int i = 0; i < k; i++)
            {
                int index = rand.Next(0, pixels.Length / 4) * 4;
                centroids.Add(Color.FromArgb(pixels[index + 3], pixels[index + 2], pixels[index + 1], pixels[index]));
            }

            return centroids;
        }

        /*private static List<Color> UpdateCentroidsAndAssignPixels(WriteableBitmap bitmap, List<Color> centroids)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int[] centroidIndices = new int[pixels.Length / 4];

            int[] totalR = new int[centroids.Count];
            int[] totalG = new int[centroids.Count];
            int[] totalB = new int[centroids.Count];
            int[] clusterCounts = new int[centroids.Count];

            for (int i = 0; i < pixels.Length; i += 4)
            {
                Color pixelColor = Color.FromArgb(pixels[i + 3], pixels[i + 2], pixels[i + 1], pixels[i]);
                Color closestCentroid = FindClosestCentroid(pixelColor, centroids);
                int closestCentroidIndex = centroids.IndexOf(closestCentroid);
                centroidIndices[i / 4] = closestCentroidIndex;

                totalR[closestCentroidIndex] += pixelColor.R;
                totalG[closestCentroidIndex] += pixelColor.G;
                totalB[closestCentroidIndex] += pixelColor.B;
                clusterCounts[closestCentroidIndex]++;
            }
            List<Color> newCentroids = new List<Color>();
            for (int i = 0; i < centroids.Count; i++)
            {
                byte newR = clusterCounts[i] == 0 ? (byte)0 : (byte)(totalR[i] / clusterCounts[i]);
                byte newG = clusterCounts[i] == 0 ? (byte)0 : (byte)(totalG[i] / clusterCounts[i]);
                byte newB = clusterCounts[i] == 0 ? (byte)0 : (byte)(totalB[i] / clusterCounts[i]);
                newCentroids.Add(Color.FromRgb(newR, newG, newB));
            }

            return newCentroids;
        }*/

        /*private static List<Color> UpdateCentroidsAndAssignPixels(WriteableBitmap bitmap, List<Color> centroids)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int[] centroidIndices = new int[width * height];
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
                    Color closestCentroid = FindClosestCentroid(pixelColor, centroids);
                    int closestCentroidIndex = centroids.IndexOf(closestCentroid);
                    centroidIndices[index] = closestCentroidIndex;

                    totalR[closestCentroidIndex] += pixelColor.R;
                    totalG[closestCentroidIndex] += pixelColor.G;
                    totalB[closestCentroidIndex] += pixelColor.B;
                    clusterCounts[closestCentroidIndex]++;
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
        }*/

        private static List<Color> UpdateCentroidsAndAssignPixels(WriteableBitmap bitmap, List<Color> centroids)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int[] centroidIndices = new int[width * height];

            byte[] pixels = GetPixels(bitmap);

            int[] totalR = new int[centroids.Count];
            int[] totalG = new int[centroids.Count];
            int[] totalB = new int[centroids.Count];
            int[] clusterCounts = new int[centroids.Count];

            int batchSize = 16;
            int numBatches = (width * height + batchSize - 1) / batchSize;

            Parallel.For(0, numBatches, batchIndex =>
            {
                int startIdx = batchIndex * batchSize;
                int endIdx = Math.Min((batchIndex + 1) * batchSize, width * height);

                for (int index = startIdx; index < endIdx; index++)
                {
                    int x = index % width;
                    int y = index / width;

                    Color pixelColor = Color.FromArgb(pixels[index * 4 + 3], pixels[index * 4 + 2], pixels[index * 4 + 1], pixels[index * 4]);
                    Color closestCentroid = FindClosestCentroid(pixelColor, centroids);
                    int closestCentroidIndex = centroids.IndexOf(closestCentroid);

                    centroidIndices[index] = closestCentroidIndex;

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

        /*private static Dictionary<Color, List<Color>> AssignPixelsToCentroids(WriteableBitmap bitmap, List<Color> centroids)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            // Use an array for centroids and another array which keeps for each pixel the index of the centroid
            Dictionary<Color, List<Color>> clusters = new Dictionary<Color, List<Color>>();

            foreach (Color centroid in centroids)
            {
                clusters[centroid] = new List<Color>();
            }

            for (int i = 0; i < pixels.Length; i += 4)
            {
                // Calulate the avg instead of assigning them
                Color pixelColor = Color.FromArgb(pixels[i + 3], pixels[i + 2], pixels[i + 1], pixels[i]);
                Color closestCentroid = FindClosestCentroid(pixelColor, centroids);
                clusters[closestCentroid].Add(pixelColor);
            }

            return clusters;
        }

        private static List<Color> UpdateCentroids(Dictionary<Color, List<Color>> clusters)
        {
            List<Color> newCentroids = new List<Color>();

            foreach (var cluster in clusters)
            {
                List<Color> pixels = cluster.Value;
                int totalR = 0, totalG = 0, totalB = 0;

                foreach (Color pixel in pixels)
                {
                    totalR += pixel.R;
                    totalG += pixel.G;
                    totalB += pixel.B;
                }

                if (pixels.Count > 0)
                {
                    byte newR = (byte)(totalR / pixels.Count);
                    byte newG = (byte)(totalG / pixels.Count);
                    byte newB = (byte)(totalB / pixels.Count);
                    newCentroids.Add(Color.FromRgb(newR, newG, newB));
                }
            }

            return newCentroids;
        }*/

        private static Color FindClosestCentroid(Color color, List<Color> centroids)
        {
            double minDistance = double.MaxValue;
            Color closestCentroid = Colors.Black;

            foreach (Color centroid in centroids)
            {
                double distance = ColorDistance(color, centroid);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCentroid = centroid;
                }
            }

            return closestCentroid;
        }

        private static double ColorDistance(Color c1, Color c2)
        {
            double dr = c1.R - c2.R;
            double dg = c1.G - c2.G;
            double db = c1.B - c2.B;
            return Math.Sqrt(dr * dr + dg * dg + db * db);
        }

        private static bool CentroidsConverged(List<Color> centroids, List<Color> newCentroids)
        {
            for (int i = 0; i < centroids.Count; i++)
            {
                if (centroids[i]!=newCentroids[i])
                    return false;
            }
            return true;
        }

        private static void ApplyFinalColors(WriteableBitmap bitmap, List<Color> centroids)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            for (int i = 0; i < pixels.Length; i += 4)
            {
                Color pixelColor = Color.FromArgb(pixels[i + 3], pixels[i + 2], pixels[i + 1], pixels[i]);
                // Use the array here
                Color closestCentroid = FindClosestCentroid(pixelColor, centroids);
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

    /*public class KMeansColorQuantization
    {
        public static BitmapSource ApplyKMeans(BitmapSource originalImage, int k, int maxIterations)
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(originalImage);

            List<Color> centroids = InitializeCentroids(writeableBitmap, k);

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                Dictionary<Color, List<Color>> clusters = AssignPixelsToCentroids(writeableBitmap, centroids);
                List<Color> newCentroids = UpdateCentroids(clusters);

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

            Random rand = new Random();
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            for (int i = 0; i < k; i++)
            {
                int x = rand.Next(0, width);
                int y = rand.Next(0, height);

                Color pixelColor = GetPixel(bitmap, x, y);
                centroids.Add(pixelColor);
            }

            return centroids;
        }

        private static Dictionary<Color, List<Color>> AssignPixelsToCentroids(WriteableBitmap bitmap, List<Color> centroids)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            Dictionary<Color, List<Color>> clusters = new Dictionary<Color, List<Color>>();

            foreach (Color centroid in centroids)
            {
                clusters[centroid] = new List<Color>();
            }

            byte[] pixels = GetPixels(bitmap);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                Color pixelColor = Color.FromArgb(pixels[i + 3], pixels[i + 2], pixels[i + 1], pixels[i]);
                Color closestCentroid = FindClosestCentroid(pixelColor, centroids);
                clusters[closestCentroid].Add(pixelColor);
            }

            return clusters;
        }

        private static Color FindClosestCentroid(Color color, List<Color> centroids)
        {
            double minDistance = double.MaxValue;
            Color closestCentroid = Colors.Black;

            foreach (Color centroid in centroids)
            {
                double distance = ColorDistance(color, centroid);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCentroid = centroid;
                }
            }

            return closestCentroid;
        }

        private static double ColorDistance(Color c1, Color c2)
        {
            double dr = c1.R - c2.R;
            double dg = c1.G - c2.G;
            double db = c1.B - c2.B;
            return Math.Sqrt(dr * dr + dg * dg + db * db);
        }

        private static List<Color> UpdateCentroids(Dictionary<Color, List<Color>> clusters)
        {
            List<Color> newCentroids = new List<Color>();

            foreach (var cluster in clusters)
            {
                List<Color> pixels = cluster.Value;
                int totalR = 0, totalG = 0, totalB = 0;

                foreach (Color pixel in pixels)
                {
                    totalR += pixel.R;
                    totalG += pixel.G;
                    totalB += pixel.B;
                }

                if (pixels.Count > 0)
                {
                    byte newR = (byte)(totalR / pixels.Count);
                    byte newG = (byte)(totalG / pixels.Count);
                    byte newB = (byte)(totalB / pixels.Count);
                    newCentroids.Add(Color.FromRgb(newR, newG, newB));
                }
            }

            return newCentroids;
        }

        private static bool CentroidsConverged(List<Color> centroids, List<Color> newCentroids)
        {
            for (int i = 0; i < centroids.Count; i++)
            {
                if (ColorDistance(centroids[i], newCentroids[i]) > 1) // Adjust the threshold as needed
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
                Color closestCentroid = FindClosestCentroid(pixelColor, centroids);
                pixels[i + 2] = closestCentroid.R;
                pixels[i + 1] = closestCentroid.G;
                pixels[i] = closestCentroid.B;
            }

            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
        }

        // Helper methods to get and set pixels from WriteableBitmap
        private static Color GetPixel(WriteableBitmap bitmap, int x, int y)
        {
            byte[] pixels = GetPixels(bitmap);
            int index = y * bitmap.PixelWidth * 4 + x * 4;
            return Color.FromArgb(pixels[index + 3], pixels[index + 2], pixels[index + 1], pixels[index]);
        }

        private static byte[] GetPixels(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            byte[] pixels = new byte[width * height * 4];
            bitmap.CopyPixels(pixels, width * 4, 0);
            return pixels;
        }
    }*/

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
