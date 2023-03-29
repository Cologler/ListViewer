using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;

using Microsoft.Extensions.DependencyInjection;

namespace ListViewer.Model
{
    class DataQueryProvider
    {
        private readonly object _syncRoot = new object();
        private readonly IServiceProvider _serviceProvider;
        private readonly CollectionDataSourceLoader _dataSourceLoader;
        private Task? _loader;

        public DataQueryProvider(IServiceProvider serviceProvider, CollectionDataSourceLoader dataSourceLoader)
        {
            this._serviceProvider = serviceProvider;
            this._dataSourceLoader = dataSourceLoader;
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

                this._dataSourceLoader.Reset();
                foreach (var dataSource in config.Sources ?? Enumerable.Empty<DataSource?>())
                {
                    if (dataSource != null)
                    {
                        await this._dataSourceLoader.AddDataSourceAsync(dataSource).ConfigureAwait(false);
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

        public async Task<QueryResult> QueryAsync(string searchText, CancellationToken cancellationToken)
        {
            await this.LoadAsync().ConfigureAwait(false);

            var config = this._serviceProvider.GetRequiredService<ConfigurationFile>();

            var searchOn = config
                .GetSearchOnColumns()?
                .Select(z => new ColumnReaderInfo(z.ColumnField ?? z.ColumnName!, z.IsContextVariable))
                .ToArray();
            var select = config.GetDisplayColumns()?
                .Select(x => new ColumnReaderInfo(x.ColumnField ?? x.ColumnName!, x.IsContextVariable)
                {
                    DisplayName = x.ColumnName ?? x.ColumnField!
                })
                .ToArray();

            if (select?.Length == 0)
                return new QueryResult(Array.Empty<QueryRecordHeader>(), Array.Empty<QueryRecordRow>());

            var queryContext = new QueryContext(searchText)
            {
                SearchOnColumns = searchOn,
                SelectColumns = select
            };

            cancellationToken.ThrowIfCancellationRequested();

            var records = await this._dataSourceLoader.QueryAsync(queryContext, cancellationToken).ConfigureAwait(false);

            var recordsWithHeaders = records.Select(x =>
            {
                var noTable = new Dictionary<string, int>();
                var headers = x.Headers.Select(z =>
                {
                    var no = noTable[z] = 1 + noTable.GetValueOrDefault(z);
                    return new QueryRecordHeader(z) { No = no - 1 }; // start from 0
                }).ToArray();

                return (x.Rows, Headers: headers);
            }).ToArray();

            var headers = recordsWithHeaders
                .SelectMany(x => x.Headers)
                .Distinct()
                .ToArray();

            var headersMap = headers
                .Select((x, i) => (Value: x, Index: i))
                .ToDictionary(x => x.Value, x => x.Index);

            var rows = recordsWithHeaders.SelectMany(x =>
            {
                var columnsMapping = new Dictionary<int, int>(x.Headers.Select((z, i) => KeyValuePair.Create(headersMap[z], i))).AsReadOnly();
                foreach (var row in x.Rows)
                {
                    row.ColumnMapping = columnsMapping;
                }
                return x.Rows;
            }).ToArray();

            return new QueryResult(headers, rows);
        }
    }
}
