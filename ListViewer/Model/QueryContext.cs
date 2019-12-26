using System.Collections.Generic;

namespace ListViewer.Model
{
    class QueryContext
    {
        public QueryContext(string searchText, IReadOnlyCollection<string> searchOn, IReadOnlyCollection<string> select)
        {
            this.SearchText = searchText;
            this.SearchOnColumns = searchOn;
            this.SelectColumns = select;
        }

        public string SearchText { get; }

        public IReadOnlyCollection<string> SearchOnColumns { get;  }

        public IReadOnlyCollection<string> SelectColumns { get; }

        public bool SearchOnAll => this.SearchOnColumns.Count == 0;
    }
}
