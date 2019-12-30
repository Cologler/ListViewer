using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;

namespace ListViewer.Model.Bases
{
    abstract class BaseDataQuerySource
    {
        private ITable? _cachedTable;

        public Dictionary<string, string> ContextFields { get; } = new Dictionary<string, string>();

        public FieldsMapper FieldsMapper { get; protected set; } = default!;

        public virtual Task LoadAsync(DataSource dataSource)
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
            this._cachedTable = table.CreateCopy();
        }

        public IEnumerable<QueryRecordRow> Query(QueryContext queryContext, CancellationToken cancellationToken)
        {
            using (var table = this.ConnectTable())
            {
                using var reader = table.OpenReader();
                var selectReaders = ColumnValueReader.CreateReaders(table, reader, queryContext.SelectColumns, this.FieldsMapper)
                    .ToArray();

                var recordValuesReader = queryContext.SearchOnAll
                    ? (IRecordSearchFieldValuesReader)reader
                    : new RecordSearchFieldValuesReader(
                        ColumnValueReader.CreateReaders(
                            table, reader, queryContext.SearchOnColumns, this.FieldsMapper)
                        .ToArray());
                var recordFilter = queryContext.RecordFilter;

                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (recordFilter.IsMatch(recordValuesReader))
                    {
                        yield return new QueryRecordRow(selectReaders.Select(z => z.ReadValue()).ToArray());
                    }
                }
            }
        }
    }
}
