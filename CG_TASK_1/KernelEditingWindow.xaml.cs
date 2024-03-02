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

namespace CG_TASK_1
{
    /// <summary>
    /// Interaction logic for KernelEditingWindow.xaml
    /// </summary>
    public partial class KernelEditingWindow : Window
    {
        private int[,] kernelValues;
        private int kernelSizeX;
        private int kernelSizeY;
        public event EventHandler<ConvAppliedEventArgs> FilterLoaded;

        public KernelEditingWindow()
        {
            InitializeComponent();
        }

        private void SetKernelSizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(KernelSizeXTextBox.Text, out kernelSizeX) && int.TryParse(KernelSizeYTextBox.Text, out kernelSizeY))
            {
                if(kernelSizeX<=9 && kernelSizeX >= 0 && kernelSizeY <=9 && kernelSizeY >= 0)
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
            kernelValues = new int[kernelSizeY, kernelSizeX];

            KernelGrid.Children.Clear();
            KernelGrid.RowDefinitions.Clear();
            KernelGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < kernelSizeY; i++)
            {
                KernelGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int j = 0; j < kernelSizeX; j++)
            {
                KernelGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < kernelSizeY; i++)
            {
                for (int j = 0; j < kernelSizeX; j++)
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
                    kernelValues[rowIndex, columnIndex] = value;
                }
            }
        }

        private System.Drawing.Point anchorPoint;

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
                            textBox.Background = System.Windows.Media.Brushes.White; // Reset other TextBoxes' color
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
            foreach (var value in kernelValues)
            {
                sum += value;
            }
            DivisorTextBox.Text = sum.ToString();
        }

        public void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (kernelValues != null)
            {
                int divisor = int.Parse(DivisorTextBox.Text);
                int offset = int.Parse(OffsetTextBox.Text);
                FilterLoaded?.Invoke(this, new ConvAppliedEventArgs(kernelValues, anchorPoint, divisor, offset));
            }
            else
            {
                MessageBox.Show("Kernel values are not loaded. Please load a filter first.");
            }
        }

        private void SaveFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Save filter logic
            // Example: Save kernel values to a file
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < kernelValues.GetLength(0); i++)
            {
                for (int j = 0; j < kernelValues.GetLength(1); j++)
                {
                    sb.Append(kernelValues[i, j]);
                    sb.Append(" ");
                }
                sb.AppendLine();
            }

            // Example: Save to a file named "filter.txt" in the application directory
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "filter.txt");
            System.IO.File.WriteAllText(filePath, sb.ToString());

            MessageBox.Show("Filter saved to filter.txt");
        }
    }
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
}
