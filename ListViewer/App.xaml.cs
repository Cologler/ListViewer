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

        protected override async void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += this.App_DispatcherUnhandledException;

            if (e.Args.Length > 1)
            {
                MessageBox.Show($"Too many arguments, expect 0 or 1, got {e.Args.Length}.");
                this.Shutdown(1);
                return;
            }

            var configFileName = e.Args.Length == 1 ? e.Args[0]! : Constants.DefaultConfigFileName;

            var text = this.ReadTextOrExit(configFileName);
            var config = this.ParseConfigurationFile(text);
            ServiceProvider = CreateFrom(config);

            try
            {
                await ServiceProvider.GetRequiredService<DataQueryProvider>()
                    .LoadAsync();
            }
            catch (BadConfigurationException exc)
            {
                this.OnException(exc);
                return;
            }
            
            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            switch (e.Exception)
            {
                case BadConfigurationException exc:
                    this.OnException(exc);
                    e.Handled = true;
                    break;

                default:
                    break;
            }
        }

        private void OnException(BadConfigurationException e)
        {
            MessageBox.Show(e.Message, "Bad configuration file");
            this.Shutdown(3);
        }

        private string ReadTextOrExit(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    return File.ReadAllText(fileName);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Unable to read file ({fileName}): \n" + e.ToString());
                }    
            }
            else
            {
                MessageBox.Show($"File ({fileName}) does not exists.");
            }

            this.Shutdown(2);
            throw new NotImplementedException();
        }

        private ConfigurationFile ParseConfigurationFile(string content)
        {
            try
            {
                return JsonSerializer.Deserialize<ConfigurationFile>(content);
            }
            catch (JsonException e)
            {
                MessageBox.Show($"Error on parse config file:\n" + e.ToString());
            }

            this.Shutdown(3);
            throw new NotImplementedException();
        }

        internal static IServiceProvider CreateFrom(ConfigurationFile config)
        {
            return new ServiceCollection()
                .AddSingleton(config)
                .AddSingleton<IEncodingResolver, EncodingResolver>()
                .AddSingleton<IDataSourceLoaderFactory, DataSourceLoaderFactory>()
                .AddSingleton<DataQueryProvider>()
                .AddTransient<DirectoryDataSourceLoader>()
                .AddTransient<IDataSourceLoader>(p => p.GetRequiredService<DirectoryDataSourceLoader>())
                .AddTransient<CsvDataSourceLoader>()
                .AddTransient<IDataSourceLoader>(p => p.GetRequiredService<CsvDataSourceLoader>())
                .AddTransient<Sqlite3DataSourceLoader>()
                .AddTransient<IDataSourceLoader>(p => p.GetRequiredService<Sqlite3DataSourceLoader>())
                .BuildServiceProvider();
        }
    }
}
