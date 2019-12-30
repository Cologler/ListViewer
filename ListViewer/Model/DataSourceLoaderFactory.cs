using System;
using System.Collections.Generic;
using System.Linq;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;
using Microsoft.Extensions.DependencyInjection;

namespace ListViewer.Model
{
    class DataSourceLoaderFactory : IDataSourceLoaderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _typesMap;

        public DataSourceLoaderFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            this._typesMap = this._serviceProvider.GetServices<IDataSourceLoader>()
                .ToDictionary(z => z.ProviderName, z => z.GetType());
        }

        public IDataSourceLoader Create(DataSource dataSource)
        {
            var providerName = dataSource.Provider;
            if (providerName is null)
                throw new BadConfigurationException("\"Provider\" cannot be null.");

            if (!this._typesMap.TryGetValue(providerName, out var type))
            {
                throw new BadConfigurationException($"unknown provider: {providerName}");
            }

            return (IDataSourceLoader)this._serviceProvider.GetRequiredService(type);
        }
    }
}
