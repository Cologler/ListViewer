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

        public List<Column> Columns { get; set; } = default!;

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

            if (this.Columns.Any(z => z is null)) 
            {
                this.Columns = this.Columns.OfType<Column>().ToList();
            }

            if (!this.Columns.Any(z => z.IsDisplay))
                throw new BadConfigurationException("No column will be display.");

            foreach (var column in this.Columns)
            {
                if (column.ColumnName is null && column.ColumnField is null)
                    throw new BadConfigurationException("ColumnField and ColumnName both are null.");
            }
        }
    }
}
