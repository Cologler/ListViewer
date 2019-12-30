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
            using (var stringReader = new StringReader(this._csvData!))
            using (var reader = new CsvReader(stringReader))
            {
                reader.Read();
                reader.ReadHeader();


                var searchOnReaders = queryContext.SearchOnColumns
                    .Select(z =>
                    {
                        return z.IsContextField
                            ? ColumnValueReader.FromContextFields(this.ContextFields, z.Key)
                            : new CsvColumnValueReader(reader, this.FieldsMapper.Get(z.Key));
                    })
                    .ToArray();

                var selectReaders = queryContext.SelectColumns
                    .Select(z =>
                    {
                        return z.IsContextField
                            ? ColumnValueReader.FromContextFields(this.ContextFields, z.Key)
                            : new CsvColumnValueReader(reader, this.FieldsMapper.Get(z.Key));
                    })
                    .ToArray();

                var recordValuesReader = queryContext.SearchOnAll
                    ? new ReadAllRecordSearchFieldValuesReader(reader) as IRecordSearchFieldValuesReader
                    : new RecordSearchFieldValuesReader(searchOnReaders);
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

        class CsvColumnValueReader : ColumnValueReader
        {
            private readonly CsvReader _reader;
            private readonly string _columnName;

            public CsvColumnValueReader(CsvReader reader, string columnName)
            {
                this._reader = reader;
                this._columnName = columnName;
            }

            public override string? TryReadValue()
            {
                return this._reader.TryGetField(this._columnName, out string v) ? v : null;
            }
        }

        class ReadAllRecordSearchFieldValuesReader : IRecordSearchFieldValuesReader
        {
            private readonly CsvReader _reader;

            public ReadAllRecordSearchFieldValuesReader(CsvReader reader)
            {
                this._reader = reader;
            }

            public IEnumerable<string> GetSearchFieldValues() =>
                Enumerable.Range(0, this._reader.Context.ColumnCount).Select(z => this._reader.GetField(z));
        }
    }
}
