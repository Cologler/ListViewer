using System.Collections.Generic;

namespace ListViewer.Abstractions
{
    interface IRecordSearchFieldValuesReader
    {
        IEnumerable<string> GetSearchFieldValues();
    }
}
