using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;
using Microsoft.Extensions.DependencyInjection;

namespace ListViewer.Model
{
    class CsvDataQuerySource : IDataQuerySource
    {
        private readonly IServiceProvider _serviceProvider;
        private string _csvData = default!;
        private FieldsMapper _fieldsMapper = default!;

        public CsvDataQuerySource(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public Task LoadAsync(DataSource dataSource)
        {
            var dataSourceView = (IDataFileDataSourceView) dataSource;
            var filePath = dataSourceView.GetFilePath();
            var encoding = this._serviceProvider.GetRequiredService<IEncodingResolver>()
                .GetEncoding(dataSourceView.Encoding);
            this._csvData = File.ReadAllText(filePath, encoding);
            this._fieldsMapper = dataSourceView.CreateFieldsMapper();
            return Task.CompletedTask;
        }

        public IEnumerable<QueryRecordRow> Query(QueryContext queryContext, CancellationToken cancellationToken)
        {
            var searchOnColumns = queryContext.SearchOnColumns.Select(z => this._fieldsMapper.Get(z)).ToArray();
            var selectColumns = queryContext.SelectColumns.Select(z => this._fieldsMapper.Get(z)).ToArray();

            using (var reader = new StringReader(this._csvData!))
            using (var csvReader = new CsvReader(reader))
            {
                csvReader.Read();
                csvReader.ReadHeader();

                bool IsMatchRow()
                {
                    if (queryContext.SearchText.Length == 0)
                        return true;

                    if (queryContext.SearchOnAll)
                    {
                        return Enumerable.Range(0, csvReader.Context.ColumnCount)
                            .Where(i => csvReader.GetField(i).Contains(queryContext.SearchText, StringComparison.OrdinalIgnoreCase))
                            .Any();
                    }
                    else
                    {
                        return queryContext.SearchOnColumns
                            .Where(name =>
                                csvReader.TryGetField(name, out string v) &&
                                v.Contains(queryContext.SearchText, StringComparison.OrdinalIgnoreCase))
                            .Any();
                    }
                }

                while (csvReader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (IsMatchRow())
                    {
                        yield return new QueryRecordRow(
                            queryContext.SelectColumns.Select(
                                name => csvReader.TryGetField(name, out string v) ? v : $"${name}").ToArray());
                    }
                }
            }
        }
    }
}
