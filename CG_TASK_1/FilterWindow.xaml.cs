using CG_TASK_1;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window
    {

        public event EventHandler<FilterAppliedEventArgs> FilterApplied;
        public FilterWindow()
        {
            InitializeComponent();
            this.Topmost = true;
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            if (FilterComboBox.SelectedIndex >= 0)
            {
                int selectedFilterIndex = FilterComboBox.SelectedIndex;
                FilterApplied?.Invoke(this, new FilterAppliedEventArgs(selectedFilterIndex));
            }
            else
            {
                MessageBox.Show("Please select a filter.");
            }
        }
    }

    public class FilterAppliedEventArgs : EventArgs
    {
        public int SelectedFilterIndex { get; }
        public double FilterParameter { get; }

        public FilterAppliedEventArgs(int selectedFilterIndex)
        {
            SelectedFilterIndex = selectedFilterIndex;
        }
    }
}
