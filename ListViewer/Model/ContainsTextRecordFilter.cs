using System;
using System.Linq;
using ListViewer.Abstractions;

namespace ListViewer.Model
{
    class ContainsTextRecordFilter : IRecordFilter
    {
        private readonly string _searchText;

        public ContainsTextRecordFilter(string searchText)
        {
            this._searchText = searchText;
        }

        public bool IsMatch(IRecordSearchFieldValuesReader reader)
        {
            return reader.GetSearchFieldValues()
                .Where(data => data.Contains(this._searchText, StringComparison.OrdinalIgnoreCase))
                .Any();
        }
    }
}
