using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;
using Microsoft.Extensions.DependencyInjection;

namespace ListViewer.Model
{
    abstract class CollectionDataSourceLoader
    {
        private readonly List<IDataSourceLoader> _subDataSourceLoaders = new List<IDataSourceLoader>();
        protected readonly IServiceProvider _serviceProvider;

        public CollectionDataSourceLoader(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        protected async Task AddSubDataQuerySourceAsync(DataSource dataSource)
        {
            var querySource = this._serviceProvider.GetRequiredService<IDataSourceLoaderFactory>().Create(dataSource);
            await querySource.ConfigureAsync(dataSource);
            this._subDataSourceLoaders.Add(querySource);
        }

        public async ValueTask<IReadOnlyList<QueryRecords>> QueryAsync(QueryContext queryContext, CancellationToken cancellationToken)
        {
            return (await Task.WhenAll(this._subDataSourceLoaders.Select(x => x.QueryAsync(queryContext, cancellationToken).AsTask())))
                .SelectMany(x => x)
                .ToArray();
        }

        protected async ValueTask<IReadOnlyList<string>> GetHeadersAsync()
        {
            return (await Task.WhenAll(this._subDataSourceLoaders.Select(x => x.GetHeadersAsync().AsTask())))
                .SelectMany(x => x)
                .Distinct()
                .ToArray();
        }
    }
}
