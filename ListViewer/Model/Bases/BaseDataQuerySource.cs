using System;
using System.Collections.Generic;
using System.Text;

namespace ListViewer.Model.Bases
{
    class BaseDataQuerySource
    {
        public Dictionary<string, string> ContextFields { get; } = new Dictionary<string, string>();

        public FieldsMapper FieldsMapper { get; protected set; } = default!;
    }
}
