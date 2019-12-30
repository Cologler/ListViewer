using System.Collections.Generic;

namespace ListViewer.Model
{
    abstract class ColumnValueReader
    {
        public abstract string? TryReadValue();

        public virtual string ReadValue() => this.TryReadValue() ?? string.Empty;

        public static ColumnValueReader FromContextFields(Dictionary<string, string> envs, string key)
        {
            return new ConstantsValueReader(envs.TryGetValue(key, out var v) ? v : $"%{key}%");
        }

        class ConstantsValueReader : ColumnValueReader
        {
            private readonly string _value;

            public ConstantsValueReader(string value)
            {
                this._value = value;
            }

            public override string TryReadValue() => this._value;
        }
    }
}
