using System;
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
        private IOpenedTables? _cachedTables;

        public Dictionary<string, string> ContextVariables { get; } = new Dictionary<string, string>();

        public FieldsMapper FieldsMapper { get; protected set; } = default!;

        public virtual async Task ConfigureAsync(DataSource dataSource)
        {
            if (dataSource.LoadEntireTableToMemory)
            {
                await this.LoadEntireTableToMemory();
            }
        }

        public virtual IOpenedTables ConnectTables() => this._cachedTables ?? this.ConnectTablesCore();

        protected abstract IOpenedTables ConnectTablesCore();

        public async Task LoadEntireTableToMemory()
        {
            using var tables = this.ConnectTablesCore();

            var cachedTables = new OpenedTables();
            await foreach (var table in tables.GetTablesAsync(default))
            {
                cachedTables.AddTable(new InMemoryTable(table));
            }
            this._cachedTables = cachedTables;
        }

        protected ValueTask<QueryRecords> QueryFromTableAsync(ITable table, QueryContext queryContext, CancellationToken cancellationToken)
        {
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

            return new (new QueryRecords(headers, Query().ToArray()));
        }

        public async ValueTask<IReadOnlyList<QueryRecords>> QueryAsync(QueryContext queryContext, CancellationToken cancellationToken)
        {
            using var tables = this.ConnectTables();

            var records = new List<QueryRecords>();
            await foreach (var table in tables.GetTablesAsync(cancellationToken))
            {
                using (table)
                {
                    records.Add(await this.QueryFromTableAsync(table, queryContext, cancellationToken).ConfigureAwait(false));
                }
            }
            return records;
        }
    }
}
