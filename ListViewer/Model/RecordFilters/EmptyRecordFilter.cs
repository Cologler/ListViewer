using ListViewer.Abstractions;

namespace ListViewer.Model.RecordFilters
{
    class EmptyRecordFilter : IRecordFilter
    {
        public bool IsMatch(IRecordSearchFieldValuesReader reader) => true;
    }
}
