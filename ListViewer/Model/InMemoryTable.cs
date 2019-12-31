﻿using System;
using System.Collections.Generic;
using ListViewer.Abstractions;

namespace ListViewer.Model
{
    class InMemoryTable : ITable
    {
        private readonly Dictionary<string, int> _headerIndexes = new Dictionary<string, int>();
        private readonly Dictionary<string, string> _contextVariables = new Dictionary<string, string>();
        private readonly List<string?[]> _rowDatas = new List<string?[]>();

        public InMemoryTable(ITable table)
        {
            this._headerIndexes = new Dictionary<string, int>(table.HeaderIndexes);
            this._contextVariables = new Dictionary<string, string>(table.ContextVariables);

            using (var reader = table.OpenReader())
            {
                while (reader.Read())
                {
                    this._rowDatas.Add(reader.GetColumnValues());
                }
            }
        }

        public IReadOnlyDictionary<string, int> HeaderIndexes => _headerIndexes;

        public IReadOnlyDictionary<string, string> ContextVariables => _contextVariables;

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
