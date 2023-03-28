using System;
using System.Configuration;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Jasily.DI.ScopedValue;
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
        private IServiceProvider _appServiceProvider = default!;
        private IServiceScope? _serviceScope;

        public new static App Current => (App)Application.Current;

        public IServiceProvider? ScopedServiceProvider => this._serviceScope?.ServiceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += this.App_DispatcherUnhandledException;

            if (e.Args.Length > 1)
            {
                MessageBox.Show($"Too many arguments, expect 0 or 1, got {e.Args.Length}.");
                this.Shutdown(1);
                return;
            }

            this._appServiceProvider = CreateServiceProvider();

            this.UseEmptyConfiguration();

            if (e.Args.Length == 1)
            {
                _ = this.TryReadConfiguration(e.Args[0]);
            }
            else if (File.Exists(Constants.DefaultConfigFileName))
            {
                _ = this.TryReadConfiguration(Constants.DefaultConfigFileName);
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

        public void OnException(BadConfigurationException e) => MessageBox.Show(e.Message, "Bad configuration file");

        public void UseEmptyConfiguration()
        {
            this._serviceScope?.Dispose();
            this._serviceScope = this._appServiceProvider.CreateScopeWithValues((typeof(ConfigurationFile), new ConfigurationFile
            {

            }));
        }

        public async Task<bool> TryReadConfiguration(string filePath)
        {
            if (filePath is null)
                throw new ArgumentNullException(nameof(filePath));

            // read file
            string? text = default;
            if (File.Exists(filePath))
            {
                try
                {
                    text = File.ReadAllText(filePath);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Unable to read file ({filePath}): \n" + e.ToString());
                }
            }
            else
            {
                MessageBox.Show($"File ({filePath}) does not exists.");
            }
            if (text is null)
                return false;

            // parse...
            ConfigurationFile? config = default;
            try
            {
                config = JsonSerializer.Deserialize<ConfigurationFile>(text);
            }
            catch (JsonException e)
            {
                MessageBox.Show($"Error on parse config file:\n" + e.ToString());
            }
            if (config is null)
                return false;

            // check...
            var scope = this._appServiceProvider.CreateScopeWithValues((typeof(ConfigurationFile), config));
            try
            {
                scope.ServiceProvider.GetRequiredService<ConfigurationFile>().Check();
                await scope.ServiceProvider.GetRequiredService<DataQueryProvider>()
                    .LoadAsync();
            }
            catch (BadConfigurationException e)
            {
                MessageBox.Show(e.Message, "Bad configuration file");
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error occur");
                return false;
            }

            this._serviceScope?.Dispose();
            this._serviceScope = scope;
            return true;
        }

        private static IServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton<IEncodingResolver, EncodingResolver>()
                .AddScopedValue<ConfigurationFile>()
                .AddScoped<IDataSourceLoaderFactory, DataSourceLoaderFactory>()
                .AddScoped<MainViewModel>()
                .AddScoped<DataQueryProvider>()
                .AddTransient<DirectoryDataSourceLoader>()
                .AddTransient<CollectionDataSourceLoader>()
                .AddTransient<IDataSourceLoader>(p => p.GetRequiredService<DirectoryDataSourceLoader>())
                .AddTransient<CsvDataSourceLoader>()
                .AddTransient<IDataSourceLoader>(p => p.GetRequiredService<CsvDataSourceLoader>())
                .AddTransient<Sqlite3DataSourceLoader>()
                .AddTransient<IDataSourceLoader>(p => p.GetRequiredService<Sqlite3DataSourceLoader>())
                .BuildServiceProvider();
        }
    }
}
