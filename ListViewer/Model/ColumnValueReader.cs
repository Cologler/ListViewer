using System;
using System.Collections.Generic;
using ListViewer.Abstractions;

namespace ListViewer.Model
{
    abstract class ColumnValueReader
    {
        public abstract string? TryReadValue();

        public virtual string ReadValue() => this.TryReadValue() ?? string.Empty;

        public static IEnumerable<ColumnValueReader> CreateReaders(ITable table, ITableRowReader tableRowReader,
            IEnumerable<ColumnReaderInfo> readerInfos, FieldsMapper fieldsMapper)
        {
            foreach (var readerInfo in readerInfos)
            {
                if (readerInfo.IsContextVariable)
                {
                    yield return FromContextFields(table.ContextVariables, readerInfo.Key);
                }
                else
                {
                    yield return FromIndex(tableRowReader,
                        table.HeaderIndexes.GetValueOrDefault(fieldsMapper.Get(readerInfo.Key), -1));
                }
            }
        }

        public static ColumnValueReader FromContextFields(IReadOnlyDictionary<string, string> envs, string key)
        {
            return FromValue(envs.TryGetValue(key, out var v) ? v : $"%{key}%");
        }

        public static ColumnValueReader FromValue(string? value) => new ConstantsValueReader(value);

        class ConstantsValueReader : ColumnValueReader
        {
            private readonly string? _value;

            public ConstantsValueReader(string? value)
            {
                this._value = value;
            }

            public override string? TryReadValue() => this._value;
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

            public override string? TryReadValue() => this._tableRowReader.GetColumnValue(this._index);
        }
    }
}
