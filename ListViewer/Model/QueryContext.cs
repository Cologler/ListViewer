using System.Collections.Generic;
using System.Dynamic;
using ListViewer.Abstractions;
using ListViewer.Model.RecordFilters;

namespace ListViewer.Model
{
    class QueryContext
    {
        public QueryContext(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                this.RecordFilter = new EmptyRecordFilter();
            }
            else
            {
                this.RecordFilter = new ContainsTextRecordFilter(searchText);
            }
        }

        public IReadOnlyCollection<ColumnReaderInfo>? SearchOnColumns { get; init; }

        public IReadOnlyCollection<ColumnReaderInfo>? SelectColumns { get; init; }

        public bool IsSearchOnAllFields => this.SearchOnColumns is null || this.SearchOnColumns.Count == 0;

        public IRecordFilter RecordFilter { get; }
    }
}
