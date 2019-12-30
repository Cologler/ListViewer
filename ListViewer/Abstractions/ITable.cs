using System;
using System.Collections.Generic;
using System.Linq;
using ListViewer.Model;

namespace ListViewer.Abstractions
{
    interface ITable : IDisposable
    {
        IReadOnlyDictionary<string, int> HeaderIndexes { get; }

        ITableRowReader OpenReader();

        IReadOnlyDictionary<string, string> ContextFields { get; }

        ITable CreateCopy() => new InMemoryTable().CacheFrom(this);
    }
}
