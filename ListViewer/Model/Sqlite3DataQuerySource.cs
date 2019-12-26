using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;

namespace ListViewer.Model
{
    class Sqlite3DataQuerySource : IDataQuerySource, IDisposable
    {
        private SQLiteConnection _sqliteConnection = default!;
        private string _table = default!;
        private FieldsMapper _fieldsMapper = default!;

        public void Dispose() => this._sqliteConnection?.Dispose();

        public Task LoadAsync(DataSource dataSource)
        {
            var dataSourceView = (ISqlite3DataSourceView)dataSource;
            this._sqliteConnection = new SQLiteConnection(dataSourceView.GetConnectionString());
            this._table = dataSourceView.GetTable();
            this._fieldsMapper = dataSourceView.CreateFieldsMapper();
            return Task.CompletedTask;
        }

        public IEnumerable<QueryRecordRow> Query(QueryContext queryContext, CancellationToken cancellationToken)
        {
            this._sqliteConnection.Open();

            try
            {
                var searchOnColumns = queryContext.SearchOnColumns.Select(z => this._fieldsMapper.Get(z)).ToArray();
                var selectColumns = queryContext.SelectColumns.Select(z => this._fieldsMapper.Get(z)).ToArray();
                var selectFromDbColumns = new HashSet<string>(searchOnColumns);
                selectFromDbColumns.UnionWith(selectColumns);

                using var command = this._sqliteConnection.CreateCommand();
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = $"select {string.Join(",", selectFromDbColumns)} from {this._table!}";
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

                using (var reader = command.ExecuteReader())
                {
                    var searchOnColumnsIndexes = searchOnColumns.Select(z => (Name: z, Index: reader.GetOrdinal(z))).ToArray();
                    var selectColumnsIndexes = selectColumns.Select(z => (Name: z, Index: reader.GetOrdinal(z))).ToArray();

                    bool IsMatchRow()
                    {
                        return queryContext.SearchText.Length == 0 ||
                            searchOnColumnsIndexes
                                .Where(z => reader.GetString(z.Index).Contains(queryContext.SearchText, StringComparison.OrdinalIgnoreCase))
                                .Any();
                    }

                    while (reader.Read())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (IsMatchRow())
                        {
                            yield return new QueryRecordRow(
                                selectColumnsIndexes
                                    .Select(z => reader.GetString(z.Index))
                                    .ToArray());
                        }
                    }
                }
            }
            finally
            {
                this._sqliteConnection.Close();
            }
        }
    }
}
