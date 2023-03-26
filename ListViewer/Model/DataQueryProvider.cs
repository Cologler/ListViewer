using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;
using Microsoft.Extensions.DependencyInjection;

namespace ListViewer.Model
{
    class DataQueryProvider : CollectionDataSourceLoader
    {
        private readonly object _syncRoot = new object();
        private Task? _loader;

        public DataQueryProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public Task LoadAsync()
        {
            async Task InternalLoadAsync()
            {
                var config = this._serviceProvider.GetRequiredService<ConfigurationFile>();
                foreach (var dataSource in config.Sources ?? Enumerable.Empty<DataSource?>())
                {
                    if (dataSource != null)
                    {
                        await this.AddSubDataQuerySourceAsync(dataSource);
                    }
                }
            }

            if (this._loader is null)
            {
                lock (this._syncRoot)
                {
                    if (this._loader is null)
                    {
                        this._loader = InternalLoadAsync();
                    }
                }
            }

            return this._loader;
        }

        public async Task<IEnumerable<QueryRecordRow>> QueryAsync(string searchText, CancellationToken cancellationToken)
        {
            await this.LoadAsync();

            var config = this._serviceProvider.GetRequiredService<ConfigurationFile>();
            if (config.Columns is null)
                return Enumerable.Empty<QueryRecordRow>();

            var searchOn = config
                .GetSearchOnColumns()
                .Select(z => new ColumnReaderInfo(z.ColumnField ?? z.ColumnName!, z.IsContextVariable))
                .ToArray();
            var select = config
                 .GetDisplayColumns()
                 .Select(z => new ColumnReaderInfo(z.ColumnField ?? z.ColumnName!, z.IsContextVariable))
                 .ToArray();

            if (select.Length == 0)
                return Enumerable.Empty<QueryRecordRow>();

            var queryContext = new QueryContext(searchText, searchOn, select);

            cancellationToken.ThrowIfCancellationRequested();

            return await Task.Run<IEnumerable<QueryRecordRow>>(
                () => this.Query(queryContext, cancellationToken).ToArray(),
                cancellationToken);
        }
    }
}
