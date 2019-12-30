using ListViewer.Abstractions;

namespace ListViewer.Model
{
    class EmptyRecordFilter : IRecordFilter
    {
        public bool IsMatch(IRecordSearchFieldValuesReader reader) => true;
    }
}
