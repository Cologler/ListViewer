using System.Collections.Generic;

namespace ListViewer.Model
{
    class FieldsMapper
    {
        private readonly Dictionary<string, string?>? _fieldMap;

        public FieldsMapper(Dictionary<string, string?>? fieldMap)
        {
            this._fieldMap = fieldMap;
        }

        public string Get(string name)
        {
            return this._fieldMap is Dictionary<string, string?> fieldMap && fieldMap.TryGetValue(name, out var v)
                ? v ?? name
                : name;
        }
    }
}
