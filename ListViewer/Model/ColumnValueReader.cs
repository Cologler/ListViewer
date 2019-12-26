using System.Collections.Generic;

namespace ListViewer.Model
{
    abstract class ColumnValueReader<T>
    {
        public abstract string? TryReadValue(T data);

        public virtual string ReadValue(T data) => this.TryReadValue(data) ?? string.Empty;

        public static ColumnValueReader<T> FromContextEnvironmentsConstants(Dictionary<string, string> envs, string key)
        {
            return new ColumnValueReader<T>.ConstantsValueReader(envs.TryGetValue(key, out var v) ? v : $"%{key}%");
        }

        class ConstantsValueReader : ColumnValueReader<T>
        {
            private readonly string _value;

            public ConstantsValueReader(string value)
            {
                this._value = value;
            }

            public override string TryReadValue(T _) => this._value;
        }
    }
}
