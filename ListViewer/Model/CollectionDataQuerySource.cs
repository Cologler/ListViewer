using System;
using System.Collections.Generic;
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
        private readonly List<IDataSourceLoader> _subDataQuerySources = new List<IDataSourceLoader>();
        protected readonly IServiceProvider _serviceProvider;

        public CollectionDataSourceLoader(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        protected async Task AddSubDataQuerySourceAsync(DataSource dataSource)
        {
            var querySource = this._serviceProvider.GetRequiredService<IDataQuerySourceFactory>().Create(dataSource);
            await querySource.ConfigureAsync(dataSource);
            this._subDataQuerySources.Add(querySource);
        }

        public IEnumerable<QueryRecordRow> Query(QueryContext queryContext, CancellationToken cancellationToken) =>
            this._subDataQuerySources.SelectMany(
                z => z.Query(queryContext, cancellationToken));
    }
}
