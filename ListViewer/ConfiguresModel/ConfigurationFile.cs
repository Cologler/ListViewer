using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace ListViewer.ConfiguresModel
{
    class ConfigurationFile
    {
        public string? Title { get; set; }

        public List<Column?>? Columns { get; set; }

        public List<DataSource?>? Sources { get; set; }

        public IReadOnlyCollection<Column> GetSearchOnColumns()
        {
            return this.Columns?
                .WhereNotNull()
                .Where(z => z.SearchOn)
                .ToArray() ?? Array.Empty<Column>();
        }

        public IReadOnlyCollection<Column> GetSelectColumns()
        {
            return this.Columns?
                .WhereNotNull()
                .Where(z => z.IsDisplay)
                .ToArray() ?? Array.Empty<Column>();
        }
    }
}
