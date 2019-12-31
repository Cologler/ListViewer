using System.Collections.Generic;
using System.Linq;
using ListViewer.Abstractions;

namespace ListViewer.Model
{
    class TableRowValuesSelector : ITableRowValuesSelector
    {
        private readonly ColumnValueReader[] _columnValueReaders;

        public TableRowValuesSelector(ColumnValueReader[] columnValueReaders)
        {
            this._columnValueReaders = columnValueReaders;
        }

        public IEnumerable<string> GetValues(ITableRowReader reader) =>
            this._columnValueReaders.Select(z => z.TryReadValue(reader)).OfType<string>();
    }
}
