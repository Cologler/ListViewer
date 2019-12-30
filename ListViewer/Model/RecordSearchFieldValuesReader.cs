using System.Collections.Generic;
using System.Linq;
using ListViewer.Abstractions;

namespace ListViewer.Model
{
    class RecordSearchFieldValuesReader<T> : IRecordSearchFieldValuesReader
    {
        private readonly T _reader;
        private readonly ColumnValueReader<T>[] _columnValueReaders;

        public RecordSearchFieldValuesReader(T reader, ColumnValueReader<T>[] columnValueReaders)
        {
            this._reader = reader;
            this._columnValueReaders = columnValueReaders;
        }

        public IEnumerable<string> GetSearchFieldValues() =>
            this._columnValueReaders.Select(z => z.TryReadValue(this._reader)).OfType<string>();
    }
}
