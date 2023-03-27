using System.Collections.Generic;

namespace ListViewer.ConfiguresModel
{
    record DataSource : DataFileTemplate,
        ISqlite3DataSourceView,
        IDataFileDataSourceView,
        IDirectoryDataSourceView
    {
        #region File

        public string? FilePath { get; set; }

        #endregion

        #region SQLite

        public string? ConnectionString { get; set; }

        public string? Table { get; set; }

        #endregion

        #region Dirs

        public string? DirPath { get; set; }

        public int? SubDirDepth { get; set; }

        public Dictionary<string, DataFileTemplate?>? Templates { get; set; }

        #endregion
    }
}
