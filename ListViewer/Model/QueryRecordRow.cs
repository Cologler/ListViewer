namespace ListViewer.Model
{
    record QueryRecordRow
    {
        public QueryRecordRow(string[] columnValues)
        {
            this.ColumnValues = columnValues;
        }

        public string[] ColumnValues { get; }

        public IReadOnlyDictionary<int, int>? ColumnMapping { get; set; }

        public string GetColumnValue(int index)
        {
            if (this.ColumnMapping is not null)
            {
                return this.ColumnMapping.TryGetValue(index, out var realIndex)
                    ? this.ColumnValues[realIndex]
                    : string.Empty;
            }
            else
            {
                if (index < this.ColumnValues.Length)
                {
                    return this.ColumnValues[index];
                }

                return string.Empty;
            }
        }
    }
}
