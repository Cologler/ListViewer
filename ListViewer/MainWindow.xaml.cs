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
            this.OnServiceProviderChanged(App.ServiceProvider);
        }

        private void OnServiceProviderChanged(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetRequiredService<ConfigurationFile>();

            this.Title = config.Title ?? "ListViewer";

            this.GridView1.Columns.Clear();            
            if (config.Columns != null)
            {
                var gvcs = config.GetDisplayColumns()
                    .Select(z => z.ColumnName ?? z.ColumnField!)
                    .Select((z, i) =>
                    {
                        return new GridViewColumn
                        {
                            Header = z,
                            DisplayMemberBinding = new Binding($"[{i}]")
                        };
                    })
                    .ToArray();
                foreach (var gvc in gvcs)
                {
                    this.GridView1.Columns.Add(gvc);
                }
            }

            var viewModel = serviceProvider.GetRequiredService<MainViewModel>();
            this.DataContext = viewModel;
            _ = viewModel.LoadAsync();
        }
    }
}
