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

        public IReadOnlyCollection<Column> GetDisplayColumns()
        {
            return this.Columns?
                .WhereNotNull()
                .Where(z => z.IsDisplay)
                .ToArray() ?? Array.Empty<Column>();
        }

        public void Check()
        {
            if (this.Columns is null)
                throw new BadConfigurationException("\"Columns\" cannot be null.");

            if (!this.Columns.OfType<Column>().Any(z => z.IsDisplay))
                throw new BadConfigurationException("none columns will be display.");
        }
    }
}
