using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ListViewer.Model;

namespace ListViewer.Abstractions
{
    interface ITable : IDisposable
    {
        ImmutableArray<string> Headers { get; }

        IReadOnlyDictionary<string, int> HeaderIndexes { get; }

        ITableRowReader OpenReader();

        IReadOnlyDictionary<string, string> ContextVariables { get; }
    }
}
