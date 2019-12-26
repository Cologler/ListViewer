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
    abstract class CollectionDataQuerySource
    {
        private readonly List<IDataQuerySource> _subDataQuerySources = new List<IDataQuerySource>();
        protected readonly IServiceProvider _serviceProvider;

        public CollectionDataQuerySource(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        protected async Task AddSubDataQuerySourceAsync(DataSource dataSource)
        {
            var querySource = this._serviceProvider.GetRequiredService<IDataQuerySourceFactory>().Create(dataSource);
            await querySource.LoadAsync(dataSource);
            this._subDataQuerySources.Add(querySource);
        }

        public IEnumerable<QueryRecordRow> Query(QueryContext queryContext, CancellationToken cancellationToken) =>
            this._subDataQuerySources.SelectMany(
                z => z.Query(queryContext, cancellationToken));
    }
}
