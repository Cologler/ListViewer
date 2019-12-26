using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;

namespace ListViewer.ConfiguresModel
{
    class ConfigurationFile
    {
        public string? Title { get; set; }

        public List<Column?>? Columns { get; set; }

        public List<DataSource?>? Sources { get; set; }
    }
}
