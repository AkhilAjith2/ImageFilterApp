using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace CG_TASK_1
{
    /// <summary>
    /// Interaction logic for KernelEditingWindow.xaml
    /// </summary>
    /// 
    public class ConvAppliedEventArgs : EventArgs
    {
        public int[,] Kernel { get; }
        public System.Drawing.Point Anchor { get; }
        public int Divisor { get; }
        public int Offset { get; }
        public ConvAppliedEventArgs(int[,] kernel, System.Drawing.Point anchor, int divisor, int offset)
        {
            Anchor = anchor;
            Kernel = kernel;
            Divisor = divisor;
            Offset = offset;
        }
    }

    public partial class KernelEditingWindow : Window
    {
        public static List<Filter> savedFilters = new List<Filter>();
        private int[,] kernelValues;
        private int kernelSizeX;
        private int kernelSizeY;
        public event EventHandler<ConvAppliedEventArgs> FilterLoaded;
        private System.Drawing.Point anchorPoint = new System.Drawing.Point(1, 1);

        public KernelEditingWindow()
        {
            InitializeComponent();
            this.Topmost = true;
            FilterComboBoxKernelEditingWindow.DataContext = FilterWindow.Instance;
            FilterComboBoxKernelEditingWindow.SelectedItem = "";
        }

        private void SetKernelSizeButton_Click(object sender, RoutedEventArgs e)
        {
            FilterComboBoxKernelEditingWindow.SelectedItem = null;
            SaveFilterName.Text = "";
            if (int.TryParse(KernelSizeXTextBox.Text, out kernelSizeX) && int.TryParse(KernelSizeYTextBox.Text, out kernelSizeY))
            {
                if (kernelSizeX <= 9 && kernelSizeX >= 0 && kernelSizeY <= 9 && kernelSizeY >= 0)
                {
                    GenerateKernelGrid();
                }
                else
                {
                    MessageBox.Show("Invalid kernel size. Please enter valid integer values.");
                }

            }
            else
            {
                MessageBox.Show("Invalid kernel size. Please enter valid integer values.");
            }
        }

        private void GenerateKernelGrid()
        {
            KernelGrid.Children.Clear();
            KernelGrid.RowDefinitions.Clear();
            KernelGrid.ColumnDefinitions.Clear();

            if (FilterComboBoxKernelEditingWindow.SelectedItem == null)
            {
                kernelValues = new int[kernelSizeX, kernelSizeY];

                for (int i = 0; i < kernelSizeX; i++)
                {
                    KernelGrid.RowDefinitions.Add(new RowDefinition());
                }

                for (int j = 0; j < kernelSizeY; j++)
                {
                    KernelGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                for (int i = 0; i < kernelSizeX; i++)
                {
                    for (int j = 0; j < kernelSizeY; j++)
                    {
                        TextBox textBox = new TextBox();
                        textBox.Text = "0";
                        textBox.TextChanged += TextBox_TextChanged;
                        textBox.Margin = new Thickness(1);
                        textBox.TextAlignment = TextAlignment.Center;
                        Grid.SetRow(textBox, i);
                        Grid.SetColumn(textBox, j);
                        KernelGrid.Children.Add(textBox);
                    }
                }
            }
            else
            {
                for (int i = 0; i < kernelValues.GetLength(0); i++)
                {
                    KernelGrid.RowDefinitions.Add(new RowDefinition());
                }

                for (int j = 0; j < kernelValues.GetLength(1); j++)
                {
                    KernelGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                for (int i = 0; i < kernelValues.GetLength(0); i++)
                {
                    for (int j = 0; j < kernelValues.GetLength(1); j++)
                    {
                        TextBox textBox = new TextBox
                        {
                            Text = kernelValues[i, j].ToString(),
                            Margin = new Thickness(1),
                            TextAlignment = TextAlignment.Center
                        };
                        textBox.TextChanged += TextBox_TextChanged;
                        Grid.SetRow(textBox, i);
                        Grid.SetColumn(textBox, j);
                        KernelGrid.Children.Add(textBox);
                    }
                }
            }
            KernelGrid.Visibility = Visibility.Visible;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                int rowIndex = Grid.GetRow(textBox);
                int columnIndex = Grid.GetColumn(textBox);
                if (int.TryParse(textBox.Text, out int value))
                {
                    kernelValues[rowIndex, columnIndex] = 0;
                    kernelValues[rowIndex, columnIndex] = value;
                }
            }
        }

        private void SetAnchorPointButton_Click(object sender, RoutedEventArgs e)
        {
            string[] anchorPointString = FilterAnchorPointTextBox.Text.Split(',');
            if (anchorPointString.Length == 2 && int.TryParse(anchorPointString[0], out int x) && int.TryParse(anchorPointString[1], out int y))
            {
                int kernelSizeX = kernelValues.GetLength(1);
                int kernelSizeY = kernelValues.GetLength(0);

                if (x >= 0 && x < kernelSizeX && y >= 0 && y < kernelSizeY)
                {
                    anchorPoint = new System.Drawing.Point(x, y);
                    foreach (var textBox in KernelGrid.Children.OfType<TextBox>())
                    {
                        int textBoxX = Grid.GetColumn(textBox);
                        int textBoxY = Grid.GetRow(textBox);

                        if (textBoxX == x && textBoxY == y)
                        {
                            textBox.Background = System.Windows.Media.Brushes.LightGreen;
                        }
                        else
                        {
                            textBox.Background = System.Windows.Media.Brushes.White;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid anchor point format. Please enter a point in the format 'x,y'.");
            }
        }

        private void AutoComputeDivisor_Click(object sender, RoutedEventArgs e)
        {
            int sum = 0;
            if(kernelValues!=null)
            {
                foreach (var value in kernelValues)
                {
                    sum += value;
                }
            }
            DivisorTextBox.Text = sum.ToString();
        }

        private void FilterComboBoxKernelEditingWindow_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterComboBoxKernelEditingWindow.SelectedItem != null)
            {
                SaveFilterName.Text = "";
                KernelSizeXTextBox.Text = "3";
                KernelSizeYTextBox.Text = "3";
                string selectedFilterName = FilterComboBoxKernelEditingWindow.SelectedItem.ToString();
                Filter selectedFilterObj = FilterManager.ConvolutionFilters.FirstOrDefault(filter => filter.Name == selectedFilterName);

                if (selectedFilterObj != null)
                {
                    // Update kernel values, anchor point, divisor, and offset based on the selected filter
                    kernelValues = selectedFilterObj.Kernel;
                    anchorPoint = selectedFilterObj.Anchor;
                    FilterAnchorPointTextBox.Text = $"{anchorPoint.X},{anchorPoint.Y}";
                    DivisorTextBox.Text = selectedFilterObj.Divisor.ToString();
                    OffsetTextBox.Text = selectedFilterObj.Offset.ToString();

                    kernelSizeY = kernelValues.GetLength(0);
                    kernelSizeX = kernelValues.GetLength(1);

                    // Generate KernelGrid with the updated kernel values
                    GenerateKernelGrid();

                    // Highlight the anchor point in KernelGrid
                    foreach (var textBox in KernelGrid.Children.OfType<TextBox>())
                    {
                        int textBoxX = Grid.GetColumn(textBox);
                        int textBoxY = Grid.GetRow(textBox);

                        if (textBoxX == anchorPoint.X && textBoxY == anchorPoint.Y)
                        {
                            textBox.Background = System.Windows.Media.Brushes.LightGreen;
                        }
                        else
                        {
                            textBox.Background = System.Windows.Media.Brushes.White;
                        }
                    }
                }
            }
        }

        public void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (kernelValues != null)
            {
                if (!string.IsNullOrWhiteSpace(DivisorTextBox.Text) && !string.IsNullOrWhiteSpace(OffsetTextBox.Text))
                {
                    int divisor, offset;
                    if (int.TryParse(DivisorTextBox.Text, out divisor) && int.TryParse(OffsetTextBox.Text, out offset))
                    {
                        FilterLoaded?.Invoke(this, new ConvAppliedEventArgs(kernelValues, anchorPoint, divisor, offset));
                    }
                    else
                    {
                        MessageBox.Show("Please enter valid integer values for divisor and offset.");
                    }
                }
                else
                {
                    MessageBox.Show("Please enter valid integer values for divisor and offset.");
                }
            }
            else
            {
                MessageBox.Show("Kernel values are not loaded. Please load a filter first.");
            }
        }


        private void SaveFilterButton_Click(object sender, RoutedEventArgs e)
        {
            string filterName = SaveFilterName.Text.Trim();

            if (!string.IsNullOrWhiteSpace(filterName) && !string.IsNullOrWhiteSpace(DivisorTextBox.Text) && !string.IsNullOrWhiteSpace(OffsetTextBox.Text))
            {
                int divisor, offset;
                if (int.TryParse(DivisorTextBox.Text, out divisor) && int.TryParse(OffsetTextBox.Text, out offset))
                {
                    Filter filter = new Filter(filterName, kernelValues, anchorPoint, divisor, offset);

                    savedFilters.Add(filter);

                    MessageBox.Show("Filter saved successfully");
                }
                else
                {
                    MessageBox.Show("Please enter valid integer values for divisor and offset.");
                }
            }
            else
            {
                MessageBox.Show("Please enter a filter name");
            }
        }
    }

}