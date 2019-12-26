using System.Collections.Generic;
using System.Dynamic;

namespace ListViewer.Model
{
    class QueryContext
    {
        public QueryContext(string searchText, IReadOnlyCollection<ColumnReaderInfo> searchOn, IReadOnlyCollection<ColumnReaderInfo> select)
        {
            this.SearchText = searchText;
            this.SearchOnColumns = searchOn;
            this.SelectColumns = select;
        }

        public string SearchText { get; }

        public IReadOnlyCollection<ColumnReaderInfo> SearchOnColumns { get;  }

        public IReadOnlyCollection<ColumnReaderInfo> SelectColumns { get; }

        public bool SearchOnAll => this.SearchOnColumns.Count == 0;
    }
}
