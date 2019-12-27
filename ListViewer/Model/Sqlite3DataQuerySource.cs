using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;
using ListViewer.Model.Bases;

namespace ListViewer.Model
{
    class Sqlite3DataQuerySource : BaseDataQuerySource, 
        IDataQuerySource, IDisposable
    {
        private SQLiteConnection _sqliteConnection = default!;
        private string _table = default!;

        public void Dispose() => this._sqliteConnection?.Dispose();

        public Task LoadAsync(DataSource dataSource)
        {
            var dataSourceView = (ISqlite3DataSourceView)dataSource;
            this._sqliteConnection = new SQLiteConnection(dataSourceView.GetConnectionString());
            this._table = dataSourceView.GetTable();
            this.FieldsMapper = dataSourceView.CreateFieldsMapper();
            return Task.CompletedTask;
        }

        public IEnumerable<QueryRecordRow> Query(QueryContext queryContext, CancellationToken cancellationToken)
        {
            this._sqliteConnection.Open();

            try
            {
                using var command = this._sqliteConnection.CreateCommand();
                var sql = $"select * from {this._table!}";
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = sql;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

                using (var reader = command.ExecuteReader())
                {
                    var searchOnReaders = queryContext.SearchOnColumns
                        .Select(z =>
                        {
                            return z.IsContextField
                                ? ColumnValueReader<SQLiteDataReader>.FromContextFields(this.ContextFields, z.Key)
                                : new SQLiteColumnValueReader(reader.GetOrdinal(this.FieldsMapper.Get(z.Key)));
                        })
                        .ToArray();

                    var selectReaders = queryContext.SelectColumns
                        .Select(z =>
                        {
                            return z.IsContextField
                                ? ColumnValueReader<SQLiteDataReader>.FromContextFields(this.ContextFields, z.Key)
                                : new SQLiteColumnValueReader(reader.GetOrdinal(this.FieldsMapper.Get(z.Key)));
                        })
                        .ToArray();

                    bool IsMatchRow()
                    {
                        if (queryContext.SearchText.Length == 0)
                            return true;

                        if (queryContext.SearchOnAll)
                        {
                            return Enumerable.Range(0, reader.FieldCount)
                                .Where(i => reader.GetString(i).Contains(queryContext.SearchText, StringComparison.OrdinalIgnoreCase))
                                .Any();
                        }
                        else
                        {
                            return searchOnReaders
                                .Where(z => 
                                    z.TryReadValue(reader) is string v &&
                                    v.Contains(queryContext.SearchText, StringComparison.OrdinalIgnoreCase))
                                .Any();
                        }
                    }

                    while (reader.Read())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (IsMatchRow())
                        {
                            yield return new QueryRecordRow(selectReaders.Select(z => z.ReadValue(reader)).ToArray());
                        }
                    }
                }
            }
            finally
            {
                this._sqliteConnection.Close();
            }
        }

        class SQLiteColumnValueReader : ColumnValueReader<SQLiteDataReader>
        {
            private readonly int _columnIndex;

            public SQLiteColumnValueReader(int columnIndex)
            {
                this._columnIndex = columnIndex;
            }

            public override string? TryReadValue(SQLiteDataReader reader)
            {
                return reader.GetString(this._columnIndex);
            }
        }
    }
}
