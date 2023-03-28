using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using ListViewer.Abstractions;

namespace ListViewer.Model
{
    class InMemoryTable : ITable
    {
        private readonly List<string?[]> _rowDatas = new List<string?[]>();

        public InMemoryTable(ITable table)
        {
            this.Headers = table.Headers;

            using (var reader = table.OpenReader())
            {
                while (reader.Read())
                {
                    this._rowDatas.Add(reader.GetColumnValues());
                }
            }
        }

        public ImmutableArray<string> Headers { get; }

        public IReadOnlyDictionary<string, string> ContextVariables { get; }

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

            public string? GetColumnValue(int index) => this._inMemoryTable._rowDatas[this._index][index];

            public string?[] GetColumnValues() => this._inMemoryTable._rowDatas[this._index];

            public bool Read()
            {
                if (this._index < this._inMemoryTable._rowDatas.Count - 1)
                {
                    this._index++;
                    return this._index < this._inMemoryTable._rowDatas.Count;
                }
                return false;
            }
        }
    }
}
