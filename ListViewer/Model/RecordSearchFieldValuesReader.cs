using System.Collections.Generic;
using System.Linq;
using ListViewer.Abstractions;

namespace ListViewer.Model
{
    class RecordSearchFieldValuesReader : IRecordSearchFieldValuesReader
    {
        private readonly ColumnValueReader[] _columnValueReaders;

        public RecordSearchFieldValuesReader(ColumnValueReader[] columnValueReaders)
        {
            this._columnValueReaders = columnValueReaders;
        }

        public IEnumerable<string> GetSearchFieldValues() =>
            this._columnValueReaders.Select(z => z.TryReadValue()).OfType<string>();
    }
}
