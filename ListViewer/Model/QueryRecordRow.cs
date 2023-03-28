namespace ListViewer.Model
{
    record QueryRecordRow
    {
        public QueryRecordRow(string[] columnValues)
        {
            this.ColumnValues = columnValues;
        }

        public string[] ColumnValues { get; }

        public (int, int)[]? ColumnMapping { get; set; }

        public string GetColumnValue(int index)
        {
            if (this.ColumnMapping is not null)
            {
                foreach (var (idx, mapTo) in this.ColumnMapping)
                {
                    if (mapTo == index)
                    {
                        return this.ColumnValues[idx];
                    }
                }
            }
            else
            {
                if (index < this.ColumnValues.Length)
                {
                    return this.ColumnValues[index];
                }
            }

            return string.Empty;
        }
    }
}
