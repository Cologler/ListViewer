using System.Collections.Generic;
using System.Dynamic;
using ListViewer.Abstractions;
using ListViewer.Model.RecordFilters;

namespace ListViewer.Model
{
    class QueryContext
    {
        public QueryContext(string searchText, IReadOnlyCollection<ColumnReaderInfo> searchOn, IReadOnlyCollection<ColumnReaderInfo> select)
        {
            this.SearchOnColumns = searchOn;
            this.SelectColumns = select;

            if (string.IsNullOrEmpty(searchText))
            {
                this.RecordFilter = new EmptyRecordFilter();
            }
            else
            {
                this.RecordFilter = new ContainsTextRecordFilter(searchText);
            }
        }

        public IReadOnlyCollection<ColumnReaderInfo> SearchOnColumns { get;  }

        public IReadOnlyCollection<ColumnReaderInfo> SelectColumns { get; }

        public bool SearchOnAll => this.SearchOnColumns.Count == 0;

        public IRecordFilter RecordFilter { get; }
    }
}
