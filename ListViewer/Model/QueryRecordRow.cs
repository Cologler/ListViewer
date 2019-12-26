namespace ListViewer.Model
{
    class QueryRecordRow
    {
        public QueryRecordRow(string[] columnValues)
        {
            this.ColumnValues = columnValues;
        }

        public string[] ColumnValues { get; }
    }
}
