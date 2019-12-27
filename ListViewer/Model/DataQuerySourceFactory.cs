using System;
using System.Collections.Generic;
using System.Linq;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;
using Microsoft.Extensions.DependencyInjection;

namespace ListViewer.Model
{
    class DataQuerySourceFactory : IDataQuerySourceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _typesMap;

        public DataQuerySourceFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            this._typesMap = this._serviceProvider.GetServices<IDataQuerySource>()
                .ToDictionary(z => z.ProviderName, z => z.GetType());
        }

        public IDataQuerySource Create(DataSource dataSource)
        {
            var providerName = dataSource.Provider;
            if (providerName is null)
                throw new BadConfigurationException("\"Provider\" cannot be null.");

            if (!this._typesMap.TryGetValue(providerName, out var type))
            {
                throw new BadConfigurationException($"unknown provider: {providerName}");
            }

            return (IDataQuerySource)this._serviceProvider.GetRequiredService(type);
        }
    }
}
