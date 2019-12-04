using System;
using System.Windows;
using System.Windows.Controls;

namespace RundownTool.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenExportButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "EXPORT",
                DefaultExt = ".CSV",
                Filter = "W2W Export CSV Documents (.csv)|*.csv"
            };

            bool? result = fileDialog.ShowDialog();
            if (result == true)
            {
                // Open document
                string filename = fileDialog.FileName;
                (DataContext as ViewModels.ViewModel).MergeExport(filename, DateTime.Now);
            }
        }

        private void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as ViewModels.ViewModel).ProcessExport();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
                (DataContext as ViewModels.ViewModel).CellEditEnding(e.Column.Header.ToString(), (e.EditingElement as TextBox).Text);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            (DataContext as ViewModels.ViewModel).SaveVehicles();
        }
    }
}
