using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ListViewer.ConfiguresModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace ListViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var provider = App.Current.ScopedServiceProvider;
            if (provider != null)
            {
                this.OnServiceProviderChanged(provider);
            }
        }

        private void OnServiceProviderChanged(IServiceProvider serviceProvider)
        {
            if (serviceProvider is null)
                throw new ArgumentNullException(nameof(serviceProvider));

            this.GridView1.Columns.Clear();

            var viewModel = serviceProvider.GetRequiredService<MainViewModel>();
            if (viewModel != this.DataContext)
            {
                (this.DataContext as MainViewModel)?.Cancel();
                this.DataContext = null;
            }

            viewModel.GridViewColumns = this.GridView1.Columns;

            var config = serviceProvider.GetRequiredService<ConfigurationFile>();
            this.Title = config.Title ?? "ListViewer";

            _ = viewModel.DataQueryProvider.ReloadAsync();

            this.DataContext = viewModel;
            _ = viewModel.LoadAsync();
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var dataStringArray = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (dataStringArray.Length == 1)
                {
                    if (await App.Current.TryReadConfiguration(dataStringArray[0]))
                    {
                        this.OnServiceProviderChanged(App.Current.ScopedServiceProvider!);
                    }
                }
            }
        }

        private void AddFilesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true,
                Filter = "Data files|*.sqlite3;*.csv;*.efu"
            };

            if (dialog.ShowDialog() == true)
            {
                var cfgFile = App.Current.ScopedServiceProvider!.GetRequiredService<ConfigurationFile>();
                cfgFile.Sources ??= new();

                cfgFile.Sources.AddRange(dialog.FileNames.Select(name =>
                {
                    bool HasExt(string ext) => name.EndsWith(ext, StringComparison.OrdinalIgnoreCase);

                    var provider = name switch
                    {
                        _ when HasExt(".sqlite3") => DataProviderNames.Sqlite3,
                        _ when HasExt(".csv") || HasExt(".efu") => DataProviderNames.Csv,
                        _ => null
                    };

                    return new DataSource
                    {
                        FilePath = dialog.FileName,
                        Provider = provider
                    };
                }));

                this.OnServiceProviderChanged(App.Current.ScopedServiceProvider!);
            }
        }
    }
}
