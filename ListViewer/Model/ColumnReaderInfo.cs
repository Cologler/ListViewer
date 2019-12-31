namespace ListViewer.Model
{
    struct ColumnReaderInfo
    {
        public ColumnReaderInfo(string key, bool isContextVariable)
        {
            this.Key = key;
            this.IsContextVariable = isContextVariable;
        }

        public string Key { get; }

        public bool IsContextVariable { get; }
    }
}
