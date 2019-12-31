using System.Collections.Generic;

namespace ListViewer.Abstractions
{
    interface ITableRowValuesSelector
    {
        IEnumerable<string> GetValues(ITableRowReader reader);
    }
}
