using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;
using ListViewer.Model.Bases;
using Microsoft.Extensions.DependencyInjection;

namespace ListViewer.Model
{
    class CsvDataSourceLoader : BaseDataSourceLoader, IDataSourceLoader
    {
        private readonly IServiceProvider _serviceProvider;
        private Encoding _encoding = default!;
        private string _filePath = default!;

        public CsvDataSourceLoader(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public string ProviderName => DataProviderNames.Csv;

        public override Task ConfigureAsync(DataSource dataSource)
        {
            var dataSourceView = (IDataFileDataSourceView) dataSource;
            var filePath = dataSourceView.GetFilePath();
            this._filePath = filePath;
            this._encoding = this._serviceProvider.GetRequiredService<IEncodingResolver>()
                .GetEncoding(dataSourceView.Encoding);
            this.FieldsMapper = dataSourceView.CreateFieldsMapper();

            this.ContextVariables["FilePath"] = filePath;
            this.ContextVariables["FileName"] = Path.GetFileName(filePath);

            return base.ConfigureAsync(dataSource);
        }

        protected override ITable ConnectTableCore()
        {
            var data = File.ReadAllText(this._filePath, this._encoding);

            var stringReader = new StringReader(data);
            var reader = new CsvReader(stringReader, CultureInfo.CurrentUICulture);
            var table = new CsvTable(reader, this.ContextVariables);
            return table;
        }

        class CsvTable : ITable
        {
            private readonly CsvReader _csvReader;
            private readonly Dictionary<string, string> _contextVariables;

            public CsvTable(CsvReader csvReader, IReadOnlyDictionary<string, string> contextVariables)
            {
                this._csvReader = csvReader;
                this._contextVariables = new Dictionary<string, string>(contextVariables);

                if (csvReader.Read() && csvReader.ReadHeader())
                {
                    this.Headers = csvReader.HeaderRecord?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
                    this.HeaderIndexes = this.Headers.Select((x, i) => (x, i)).ToDictionary(x => x.x, x => x.i);
                }
                else
                {
                    this.HeaderIndexes = new Dictionary<string, int>();
                }
            }

            public ImmutableArray<string> Headers { get; }

            public IReadOnlyDictionary<string, int> HeaderIndexes { get; }

            public IReadOnlyDictionary<string, string> ContextVariables => this._contextVariables;

            void IDisposable.Dispose() => this._csvReader.Dispose();

            public ITableRowReader OpenReader() => new CsvTableRowReader(this._csvReader);
        }

        class CsvTableRowReader : ITableRowReader
        {
            private readonly CsvReader _csvReader;

            public CsvTableRowReader(CsvReader csvReader)
            {
                this._csvReader = csvReader;
            }

            public void Dispose() { }

            public string? GetColumnValue(int index) => this._csvReader.GetField(index);

            public string?[] GetColumnValues() => this._csvReader.Parser.Record ?? Array.Empty<string>();

            public bool Read() => this._csvReader.Read();
        }
    }
}
