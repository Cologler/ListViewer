namespace ListViewer.Model
{
    record struct ColumnReaderInfo(string Key, bool IsContextVariable)
    {
        public string? DisplayName { get; set; }
    }
}
