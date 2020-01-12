using System;
using System.Collections.Generic;
using ListViewer.Abstractions;

namespace ListViewer.Model
{
    abstract class ColumnValueReader
    {
        public abstract string? TryReadValue(ITableRowReader reader);

        public virtual string ReadValue(ITableRowReader reader) => this.TryReadValue(reader) ?? string.Empty;

        public static IEnumerable<ColumnValueReader> CreateReaders(ITable table, ITableRowReader tableRowReader,
            IEnumerable<ColumnReaderInfo> readerInfos, FieldsMapper fieldsMapper)
        {
            foreach (var readerInfo in readerInfos)
            {
                if (readerInfo.IsContextVariable)
                {
                    yield return FromValue(table.ContextVariables.TryGetValue(readerInfo.Key, out var v) ? v : $"%{readerInfo.Key}%");
                }
                else
                {
                    yield return FromIndex(tableRowReader,
                        table.HeaderIndexes.GetValueOrDefault(fieldsMapper.Get(readerInfo.Key), -1));
                }
            }
        }

        public static ColumnValueReader FromValue(string? value) => new ConstantsValueReader(value);

        class ConstantsValueReader : ColumnValueReader
        {
            private readonly string? _value;

            public ConstantsValueReader(string? value)
            {
                this._value = value;
            }

            public override string? TryReadValue(ITableRowReader reader) => this._value;
        }

        public static ColumnValueReader FromIndex(ITableRowReader tableRowReader, int index) => index < 0
            ? FromValue(null)
            : new GetIndexValueReader(tableRowReader, index);

        class GetIndexValueReader : ColumnValueReader
        {
            private readonly ITableRowReader _tableRowReader;
            private readonly int _index;

            public GetIndexValueReader(ITableRowReader tableRowReader, int index)
            {
                this._tableRowReader = tableRowReader;
                this._index = index;
            }

            public override string? TryReadValue(ITableRowReader reader) => this._tableRowReader.GetColumnValue(this._index);
        }
    }
}
