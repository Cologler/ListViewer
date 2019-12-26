using System.Collections.Generic;

namespace ListViewer.ConfiguresModel
{
    interface IDirectoryDataSourceView : IDataSourceView
    {
        string? DirPath { get; }

        int? SubDirDepth { get; }

        Dictionary<string, DataFileTemplate?>? Templates { get; }
    }
}
