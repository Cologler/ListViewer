using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;
using ListViewer.Model.Bases;
using Microsoft.Extensions.DependencyInjection;

namespace ListViewer.Model
{
    class CsvDataQuerySource : BaseDataQuerySource, IDataQuerySource
    {
        private readonly IServiceProvider _serviceProvider;
        private string _csvData = default!;

        public CsvDataQuerySource(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public string ProviderName => DataProviderNames.Csv;

        public Task LoadAsync(DataSource dataSource)
        {
            var dataSourceView = (IDataFileDataSourceView) dataSource;
            var filePath = dataSourceView.GetFilePath();
            var encoding = this._serviceProvider.GetRequiredService<IEncodingResolver>()
                .GetEncoding(dataSourceView.Encoding);
            this._csvData = File.ReadAllText(filePath, encoding);
            this.FieldsMapper = dataSourceView.CreateFieldsMapper();
            this.ContextFields["FilePath"] = filePath;
            this.ContextFields["FileName"] = Path.GetFileName(filePath);
            return Task.CompletedTask;
        }

        public IEnumerable<QueryRecordRow> Query(QueryContext queryContext, CancellationToken cancellationToken)
        {
            var searchOnReaders = queryContext.SearchOnColumns
                .Select(z =>
                {
                    return z.IsContextField
                        ? ColumnValueReader<CsvReader>.FromContextFields(this.ContextFields, z.Key)
                        : new CsvColumnValueReader(this.FieldsMapper.Get(z.Key));
                })
                .ToArray();

            var selectReaders = queryContext.SelectColumns
                .Select(z => 
                {
                    return z.IsContextField
                        ? ColumnValueReader<CsvReader>.FromContextFields(this.ContextFields, z.Key)
                        : new CsvColumnValueReader(this.FieldsMapper.Get(z.Key));
                })
                .ToArray();

            using (var stringReader = new StringReader(this._csvData!))
            using (var reader = new CsvReader(stringReader))
            {
                reader.Read();
                reader.ReadHeader();

                bool IsMatchRow()
                {
                    if (queryContext.SearchText.Length == 0)
                        return true;

                    if (queryContext.SearchOnAll)
                    {
                        return Enumerable.Range(0, reader.Context.ColumnCount)
                            .Where(i => reader.GetField(i).Contains(queryContext.SearchText, StringComparison.OrdinalIgnoreCase))
                            .Any();
                    }
                    else
                    {
                        return searchOnReaders
                            .Where(z => z.TryReadValue(reader) is string v &&
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

        class CsvColumnValueReader : ColumnValueReader<CsvReader>
        {
            private readonly string _columnName;

            public CsvColumnValueReader(string columnName)
            {
                this._columnName = columnName;
            }

            public override string? TryReadValue(CsvReader reader)
            {
                return reader.TryGetField(this._columnName, out string v) ? v : null;
            }
        }
    }
}
