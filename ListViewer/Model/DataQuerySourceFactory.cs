using System;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;
using Microsoft.Extensions.DependencyInjection;

namespace ListViewer.Model
{
    class DataQuerySourceFactory : IDataQuerySourceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DataQuerySourceFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public IDataQuerySource Create(DataSource dataSource)
        {
            switch (dataSource.Provider)
            {
                case DataProviderNames.Csv:
                    return this._serviceProvider.GetRequiredService<CsvDataQuerySource>();

                case DataProviderNames.Sqlite3:
                    return this._serviceProvider.GetRequiredService<Sqlite3DataQuerySource>();

                case DataProviderNames.Directory:
                    return this._serviceProvider.GetRequiredService<DirectoryDataQuerySource>();

                default:
                    throw new BadConfigurationException($"unknown provider: {dataSource.Provider}");
            }
        }
    }
}
