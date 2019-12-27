﻿using System;
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
    class Sqlite3DataQuerySource : BaseDataQuerySource, 
        IDataQuerySource
    {
        private string _connectionString = default!;
        private string _table = default!;

        public Task LoadAsync(DataSource dataSource)
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
                    throw new BadConfigurationException($"No such table ({this._table}) in database ({this._connectionString}).");
                }
            }

            return Task.CompletedTask;
        }

        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(this._connectionString);
            connection.Open();
            return connection;
        }

        public IEnumerable<QueryRecordRow> Query(QueryContext queryContext, CancellationToken cancellationToken)
        {
            using (var connection = this.OpenConnection())
            {
                using var command = connection.CreateCommand();
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