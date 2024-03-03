using CG_TASK_1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<string> FilterNames { get; } = new ObservableCollection<string>();
        public FilterWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Topmost = true;
            LoadFilters();
        }

        private void LoadFilters()
        {
            // Add predefined filter names
            string[] predefinedFilters = { "Select Filter", "Inversion", "Brightness Correction", "Contrast Enhancement", "Gamma Correction", "Blur", "Gaussian Blur", "Sharpen", "Edge Detection", "Emboss" };
            foreach (string filter in predefinedFilters)
            {
                FilterNames.Add(filter);
            }

            // Add user-defined filter names
            foreach (Filter filter in KernelEditingWindow.savedFilters)
            {
                FilterNames.Add(filter.Name);
            }
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

        public FilterAppliedEventArgs(int selectedFilterIndex)
        {
            SelectedFilterIndex = selectedFilterIndex;
        }
    }
}
