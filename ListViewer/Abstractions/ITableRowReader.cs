using System;

namespace ListViewer.Abstractions
{
    interface ITableRowReader : IDisposable
    {
        bool Read();

        string? GetColumnValue(int index);

        string?[] GetColumnValues();
    }
}
