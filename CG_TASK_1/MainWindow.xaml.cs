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
        private Stack<Bitmap> filterStack = new Stack<Bitmap>();

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
                originalBitmap = Filters.ConvertBitmapToBitmapImage(originalImage);
                OriginalImage.Source = originalBitmap;
                FilteredImage.Source = originalBitmap;
            }
            filterStack.Clear();
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
                Bitmap filteredImageCopy = Filters.ApplyFilter(filteredImage, e.SelectedFilterIndex);
                filteredBitmap = Filters.ConvertBitmapToBitmapImage(filteredImageCopy);
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
                    BitmapImage previousBitmap = Filters.ConvertBitmapToBitmapImage(filterStack.Peek());
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

    }
}
