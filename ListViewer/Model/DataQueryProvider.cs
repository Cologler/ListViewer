using System;
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
                }
                else
                {
                    this._select = config.GetDisplayColumns()?
                        .Select(x => new ColumnReaderInfo(x.ColumnField ?? x.ColumnName!, x.IsContextVariable)
                        {
                            DisplayName = x.ColumnName ?? x.ColumnField!
                        })
                        .ToArray();
                    this._searchOn = config
                        .GetSearchOnColumns()?
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

        public async Task<QueryRecords> QueryAsync(string searchText, CancellationToken cancellationToken)
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
                return new QueryRecords(ImmutableArray<string>.Empty, Array.Empty<QueryRecordRow>());

            var queryContext = new QueryContext(searchText)
            {
                SearchOnColumns = searchOn,
                SelectColumns = select
            };

            cancellationToken.ThrowIfCancellationRequested();

            var allRecords = await this.QueryAsync(queryContext, cancellationToken).ConfigureAwait(false);

            var headers = allRecords.SelectMany(x => x.Headers)
                    .Distinct()
                    .ToImmutableArray();

            var headersMap = headers
                .Select((x, i) => (Value: x, Index: i))
                .ToDictionary(x => x.Value, x => x.Index);

            var allRows = allRecords.Select(x =>
            {
                var headerMap = x.Headers.Select(z => headersMap[z]).ToArray();
                return x.Rows.Select(r =>
                {
                    var rows = new string[headers.Length];
                    Array.Fill(rows, string.Empty);
                    _ = r.ColumnValues.Select((z, i) => rows[headerMap[i]] = z).ToArray();
                    return new QueryRecordRow(rows);
                });
            }).SelectMany(x => x).ToArray();

            return new QueryRecords(headers, allRows);
        }
    }
}
