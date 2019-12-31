using System;
using System.Linq;
using ListViewer.Abstractions;

namespace ListViewer.Model.RecordFilters
{
    class ContainsTextRecordFilter : IRecordFilter
    {
        private readonly string _searchText;

        public ContainsTextRecordFilter(string searchText)
        {
            this._searchText = searchText;
        }

        public bool IsMatch(ITableRowReader reader, ITableRowValuesSelector selector)
        {
            return selector.GetValues(reader)
                .Where(data => data.Contains(this._searchText, StringComparison.OrdinalIgnoreCase))
                .Any();
        }
    }
}
