﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;

namespace ListViewer.Model.Bases
{
    abstract class BaseDataSourceLoader
    {
        private ITable? _cachedTable;

        public Dictionary<string, string> ContextVariables { get; } = new Dictionary<string, string>();

        public FieldsMapper FieldsMapper { get; protected set; } = default!;

        public virtual Task ConfigureAsync(DataSource dataSource)
        {
            if (dataSource.LoadEntireTableToMemory)
            {
                this.LoadEntireTableToMemory();
            }

            return Task.CompletedTask;
        }

        public virtual ITable ConnectTable() => this._cachedTable ?? this.ConnectTableCore();

        protected abstract ITable ConnectTableCore();

        public void LoadEntireTableToMemory()
        {
            using var table = this.ConnectTableCore();
            this._cachedTable = new InMemoryTable(table);
        }

        public ValueTask<IReadOnlyList<string>> GetHeadersAsync()
        {
            using var table = this.ConnectTableCore();
            return new(table.Headers);
        }

        public ValueTask<IReadOnlyList<QueryRecords>> QueryAsync(QueryContext queryContext, CancellationToken cancellationToken)
        {
            using var table = this.ConnectTable();

            var selectColumns = queryContext.SelectColumns;
            var headers = selectColumns?.Select(x => x.DisplayName!).ToImmutableArray() ?? table.Headers;

            IEnumerable<QueryRecordRow> Query()
            {
                var selectReaders = (selectColumns is null
                    ? ColumnValueReader.CreateReaders(table) : ColumnValueReader.CreateReaders(table, selectColumns, this.FieldsMapper)).ToArray();

                using var reader = table.OpenReader();

                var recordValuesReader = queryContext.IsSearchOnAllFields
                    ? new TableRowEntireValuesSelector() as ITableRowValuesSelector
                    : new TableRowValuesSelector(
                        ColumnValueReader.CreateReaders(
                            table, queryContext.SearchOnColumns!, this.FieldsMapper)
                        .ToArray());
                var recordFilter = queryContext.RecordFilter;

                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (recordFilter.IsMatch(reader, recordValuesReader))
                    {
                        yield return new QueryRecordRow(selectReaders.Select(z => z.ReadValue(reader)).ToArray());
                    }
                }
            }

            return new(new[] { new QueryRecords(headers, Query().ToArray()) });
        }
    }
}
