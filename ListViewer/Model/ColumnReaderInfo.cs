namespace ListViewer.Model
{
    struct ColumnReaderInfo
    {
        public ColumnReaderInfo(string key, bool isContextField)
        {
            this.Key = key;
            this.IsContextField = isContextField;
        }

        public string Key { get; }

        public bool IsContextField { get; }
    }
}
