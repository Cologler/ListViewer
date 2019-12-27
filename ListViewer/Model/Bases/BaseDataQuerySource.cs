using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ListViewer.Model.Bases
{
    class BaseDataQuerySource
    {
        public Dictionary<string, string> ContextFields { get; } = new Dictionary<string, string>();

        public FieldsMapper FieldsMapper { get; protected set; } = default!;

        protected bool IsDatasMatch(string searchText, IEnumerable<string> searchOnDatas)
        {
            return searchOnDatas
                .Where(data => data.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Any();
        }
    }
}
