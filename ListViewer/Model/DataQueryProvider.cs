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
        private ColumnReaderInfo[]? _select;
        private ColumnReaderInfo[]? _searchOn;

        public DataQueryProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public Task ReloadAsync()
        {
            this._loader = null;
            return this.LoadAsync();
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
                        await this.AddSubDataQuerySourceAsync(dataSource).ConfigureAwait(false);
                    }
                }

                if (config.Columns is null)
                {
                    this.DisplayHeaders = (await this.GetHeadersAsync().ConfigureAwait(false)).ToArray();
                    this._searchOn = this._select = this.DisplayHeaders
                        .Select(z => new ColumnReaderInfo(z, false))
                        .ToArray();
                }
                else
                {
                    var columns = config.GetDisplayColumns()!;
                    this.DisplayHeaders = columns.Select(x => x.ColumnName ?? x.ColumnField!).ToArray();
                    this._select = columns
                        .Select(x => new ColumnReaderInfo(x.ColumnField ?? x.ColumnName!, x.IsContextVariable))
                        .ToArray();
                    this._searchOn = config
                        .GetSearchOnColumns()!
                        .Select(z => new ColumnReaderInfo(z.ColumnField ?? z.ColumnName!, z.IsContextVariable))
                        .ToArray();
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

        public string[]? DisplayHeaders { get; private set; }

        public async Task<IReadOnlyList<QueryRecordRow>> QueryAsync(string searchText, CancellationToken cancellationToken)
        {
            await this.LoadAsync().ConfigureAwait(false);

            var searchOn = this._searchOn!;
            var select = this._select!;

            if (select.Length == 0)
                return Array.Empty<QueryRecordRow>();

            var queryContext = new QueryContext(searchText, searchOn, select);

            cancellationToken.ThrowIfCancellationRequested();

            return await this.QueryAsync(queryContext, cancellationToken);
        }
    }
}
