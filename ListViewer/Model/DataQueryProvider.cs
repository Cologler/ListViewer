﻿using System;
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

            var allRecords = await this._dataSourceLoader.QueryAsync(queryContext, cancellationToken).ConfigureAwait(false);

            var headers = allRecords.SelectMany(x => x.Headers)
                    .Distinct()
                    .ToImmutableArray();

            var headersMap = headers
                .Select((x, i) => (Value: x, Index: i))
                .ToDictionary(x => x.Value, x => x.Index);

            var allRows = allRecords.Select(x =>
            {
                var columnsMapping = x.Headers.Select((z, i) => (i, headersMap[z])).ToArray();
                var headerMap = x.Headers.Select(z => headersMap[z]).ToArray();
                foreach (var row in x.Rows)
                {
                    row.ColumnMapping = columnsMapping;
                }
                return x.Rows;
            }).SelectMany(x => x).ToArray();

            return new QueryRecords(headers, allRows);
        }
    }
}
