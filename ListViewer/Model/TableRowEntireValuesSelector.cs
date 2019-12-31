using System.Collections.Generic;
using System.Linq;
using ListViewer.Abstractions;

namespace ListViewer.Model
{
    class TableRowEntireValuesSelector : ITableRowValuesSelector
    {
        public IEnumerable<string> GetValues(ITableRowReader reader) => reader.GetColumnValues().OfType<string>();
    }
}
