using System.Collections.Generic;
using ListViewer.Model;

namespace ListViewer.ConfiguresModel
{
    interface IDataSourceView
    {
        string? Provider { get; }

        public Dictionary<string, string?>? FieldMap { get; }

        FieldsMapper CreateFieldsMapper() => new FieldsMapper(this.FieldMap);
    }
}
