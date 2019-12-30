using System;
using System.Collections.Generic;
using ListViewer.Abstractions;

namespace ListViewer.Model
{
    class InMemoryTable : ITable
    {
        private readonly Dictionary<string, int> _headerIndexes = new Dictionary<string, int>();
        private readonly Dictionary<string, string> _contextFields = new Dictionary<string, string>();
        private readonly List<string?[]> _rowsData = new List<string?[]>();

        public ITable CacheFrom(ITable table)
        {
            foreach (var (k, v) in table.ContextFields)
            {
                this._contextFields.Add(k, v);
            }
            foreach (var (k, v) in table.HeaderIndexes)
            {
                this._headerIndexes.Add(k, v);
            }
            using (var reader = table.OpenReader())
            {
                while (reader.Read())
                {
                    this._rowsData.Add(reader.GetColumnValues());
                }
            }
            return this;
        }

        public IReadOnlyDictionary<string, int> HeaderIndexes => _headerIndexes;

        public IReadOnlyDictionary<string, string> ContextFields => _contextFields;

        public void Dispose() { }

        public ITableRowReader OpenReader() => new TableRowReader(this);

        class TableRowReader : ITableRowReader
        {
            private readonly InMemoryTable _inMemoryTable;
            private int _index = -1;

            public TableRowReader(InMemoryTable inMemoryTable)
            {
                this._inMemoryTable = inMemoryTable;
            }

            public void Dispose() { }

            public string? GetColumnValue(int index) => this._inMemoryTable._rowsData[this._index][index];

            public string?[] GetColumnValues() => this._inMemoryTable._rowsData[this._index];

            public bool Read()
            {
                if (this._index < this._inMemoryTable._rowsData.Count - 1)
                {
                    this._index++;
                    return this._index < this._inMemoryTable._rowsData.Count;
                }
                return false;
            }
        }
    }
}
