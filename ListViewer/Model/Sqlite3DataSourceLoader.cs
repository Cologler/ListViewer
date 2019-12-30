using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
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
        private string _table = default!;

        public string ProviderName => DataProviderNames.Sqlite3;

        public override Task ConfigureAsync(DataSource dataSource)
        {
            var dataSourceView = (ISqlite3DataSourceView)dataSource;
            this._connectionString = dataSourceView.GetConnectionString();
            this._table = dataSourceView.GetTable();
            this.FieldsMapper = dataSourceView.CreateFieldsMapper();

            using (var connection = this.OpenConnection())
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1";

                using var reader = command.ExecuteReader();
                var tables = reader.OfType<IDataRecord>()
                    .Select(z => z.GetString(0))
                    .ToHashSet();

                if (!tables.Contains(this._table))
                {
                    var message = $"No such table ({this._table}) in database ({this._connectionString}).";
                    message += "\n\nAll available tables:\n" + string.Join("\n", tables.Select(z => "    - " + z));
                    throw new BadConfigurationException(message);
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

        protected override ITable ConnectTableCore()
        {
            var connection = this.OpenConnection();

            using var command = connection.CreateCommand();
            var sql = $"select * from {this._table!}";
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            command.CommandText = sql;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

            var reader = command.ExecuteReader();
            return new SQLiteTable(connection, reader);
        }

        class SQLiteTable : ITable
        {
            private readonly SQLiteConnection _connection;
            private readonly SQLiteDataReader _reader;
            private readonly Dictionary<string, int> _headerIndexes = new Dictionary<string, int>();

            public SQLiteTable(SQLiteConnection connection, SQLiteDataReader reader)
            {
                this._connection = connection;
                this._reader = reader;
                foreach (var index in Enumerable.Range(0, reader.FieldCount))
                {
                    this._headerIndexes.Add(reader.GetName(index), index);
                }
            }

            public IReadOnlyDictionary<string, int> HeaderIndexes => this._headerIndexes;

            public Dictionary<string, string> ContextFields { get; } = new Dictionary<string, string>();

            IReadOnlyDictionary<string, string> ITable.ContextFields => this.ContextFields;

            void IDisposable.Dispose()
            {
                this._reader.Dispose();
                this._connection.Dispose();
            }

            public ITableRowReader OpenReader() => new SQLiteTableRowReader(this._reader);
        }

        class SQLiteTableRowReader : ITableRowReader, IRecordSearchFieldValuesReader
        {
            private readonly SQLiteDataReader _reader;

            public SQLiteTableRowReader(SQLiteDataReader reader)
            {
                this._reader = reader;
            }

            public void Dispose() { }

            public string? GetColumnValue(int index) => this._reader.GetValue(index)?.ToString();

            public string?[] GetColumnValues() =>
                Enumerable.Range(0, this._reader.FieldCount).Select(z => this._reader.GetValue(z)?.ToString()).ToArray();

            public IEnumerable<string> GetSearchFieldValues() => this.GetColumnValues().OfType<string>();

            public bool Read() => this._reader.Read();
        }
    }
}
