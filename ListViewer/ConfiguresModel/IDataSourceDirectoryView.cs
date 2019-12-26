using System.Collections.Generic;

namespace ListViewer.ConfiguresModel
{
    interface IDataSourceDirectoryView : IDataSourceView
    {
        string? DirPath { get; }

        int? SubDirDepth { get; }

        Dictionary<string, DataFileTemplate?>? Templates { get; }
    }
}
