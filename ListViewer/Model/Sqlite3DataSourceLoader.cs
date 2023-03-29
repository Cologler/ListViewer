using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;
using ListViewer.Model.Bases;

namespace ListViewer.Model
{
    class Sqlite3DataSourceLoader : BaseDataSourceLoader, 
        IDataSourceLoader
    {
        private string _connectionString = default!;
        private string[]? _tables;

        public string ProviderName => DataProviderNames.Sqlite3;

        public override Task ConfigureAsync(DataSource dataSource)
        {
            var dataSourceView = (ISqlite3DataSourceView)dataSource;
            this._connectionString = dataSourceView.GetConnectionString();
            this.FieldsMapper = dataSourceView.CreateFieldsMapper();

            using (var connection = this.OpenConnection())
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1";

                using var reader = command.ExecuteReader();

                var tables = reader.OfType<IDataRecord>()
                    .Select(z => z.GetString(0))
                    .ToArray();

                var tablesSet = tables.ToHashSet();

                if (dataSourceView.Table is { } table)
                {
                    if (!tablesSet.Contains(table))
                    {
                        var message = $"No such table ({table}) in database ({this._connectionString}).";
                        message += "\n\nAll available tables:\n" + string.Join("\n", tablesSet.Select(z => "    - " + z));
                        throw new BadConfigurationException(message);
                    }

                    this._tables = new[] { table }; 
                }
                else
                {
                    this._tables = tables.Where(x => !x.StartsWith("sqlite_")).ToArray();
                }
            }

            return base.ConfigureAsync(dataSource);
        }

        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(this._connectionString);
            connection.Open();
            return connection;
        }

        protected override IOpenedTables ConnectTablesCore() 
            => new SQLiteTables(this.OpenConnection(), this._tables!);

        class SQLiteTables : IOpenedTables
        {
            private readonly SQLiteConnection _connection;
            private readonly string[] _tables;

            public SQLiteTables(SQLiteConnection connection, string[] tables)
            {
                this._connection = connection;
                this._tables = tables;
            }

            public void Dispose() => this._connection.Dispose();

            public async IAsyncEnumerable<ITable> GetTablesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
            {
                var connection = this._connection;
                var includeTableName = this._tables.Length > 1;

                foreach (var table in this._tables)
                {
                    using var command = connection.CreateCommand();
                    var sql = $"select * from \"{table}\"";
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    command.CommandText = sql;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

                    var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                    yield return new SQLiteTable(reader, table, includeTableName);
                }
            }
        }

        class SQLiteTable : ITable
        {
            private readonly DbDataReader _reader;
            private readonly string[] _prefixs;

            public SQLiteTable(DbDataReader reader, string tableName, bool includeTableName)
            {
                this._reader = reader;

                var fields = new List<string>();
                if (includeTableName)
                {
                    fields.Add("$Table");
                    this._prefixs = new[] { tableName };
                }
                else
                {
                    this._prefixs = Array.Empty<string>();
                } 

                fields.AddRange(Enumerable.Range(0, reader.FieldCount).Select(reader.GetName));

                this.Headers = fields.ToImmutableArray();
            }

            public ImmutableArray<string> Headers { get; }

            public Dictionary<string, string> ContextFields { get; } = new Dictionary<string, string>();

            IReadOnlyDictionary<string, string> ITable.ContextVariables => this.ContextFields;

            void IDisposable.Dispose() => this._reader.Dispose();

            public ITableRowReader OpenReader() => new SQLiteTableRowReader(this._reader, this._prefixs);
        }

        class SQLiteTableRowReader : ITableRowReader
        {
            private readonly DbDataReader _reader;
            private readonly string[] _prefixs;

            public SQLiteTableRowReader(DbDataReader reader, string[] prefixs)
            {
                this._reader = reader;
                this._prefixs = prefixs;
            }

            public void Dispose() { }

            public string? GetColumnValue(int index)
            {
                var indexOfFields = index - this._prefixs.Length;
                return indexOfFields < 0 ? this._prefixs[index] : (this._reader.GetValue(indexOfFields)?.ToString());
            }

            public string?[] GetColumnValues() 
                => this._prefixs.Concat(Enumerable.Range(0, this._reader.FieldCount).Select(z => this._reader.GetValue(z)?.ToString())).ToArray();

            public bool Read() => this._reader.Read();
        }
    }
}
