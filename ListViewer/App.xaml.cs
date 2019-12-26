using System;
using System.Configuration;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;
using ListViewer.Model;
using Microsoft.Extensions.DependencyInjection;

namespace ListViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = default!;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 1)
            {
                MessageBox.Show($"Too many arguments, expect 0 or 1, got {e.Args.Length}.");
                this.Shutdown(1);
                return;
            }

            var configFileName = e.Args.Length == 1 ? e.Args[0]! : Constants.DefaultConfigFileName;

            if (!File.Exists(configFileName))
            {
                MessageBox.Show($"Config file {configFileName} does not exists.");
                this.Shutdown(2);
                return;
            }

            var text = File.ReadAllText(configFileName);
            var config = JsonSerializer.Deserialize<ConfigurationFile>(text);

            ServiceProvider = new ServiceCollection()
                .AddSingleton(config)
                .AddSingleton<IEncodingResolver, EncodingResolver>()
                .AddSingleton<IDataQuerySourceFactory, DataQuerySourceFactory>()
                .AddSingleton<DataQueryProvider>()
                .AddTransient<DirectoryDataQuerySource>()
                .AddTransient<CsvDataQuerySource>()
                .AddTransient<Sqlite3DataQuerySource>()
                .BuildServiceProvider();
            
            base.OnStartup(e);
        }
    }
}
