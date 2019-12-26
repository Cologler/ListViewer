using System.Collections.Generic;

namespace ListViewer.ConfiguresModel
{
    interface IDataSourceView
    {
        string? Provider { get; }

        public Dictionary<string, string?>? FieldMap { get; }
    }
}
