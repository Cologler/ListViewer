using System.Collections.Generic;

namespace ListViewer.ConfiguresModel
{
    class DataSource : DataFileTemplate,
        IDataSourceSqlite3View,
        IDataSourceDataFileView,
        IDataSourceDirectoryView
    {
        public string? ConnectionString { get; set; }

        public string? Table { get; set; }

        public string? FilePath { get; set; }

        public string? DirPath { get; set; }

        public int? SubDirDepth { get; set; }

        public Dictionary<string, DataFileTemplate?>? Templates { get; set; }
    }
}
