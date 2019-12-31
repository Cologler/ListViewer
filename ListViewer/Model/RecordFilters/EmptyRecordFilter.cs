using ListViewer.Abstractions;

namespace ListViewer.Model.RecordFilters
{
    class EmptyRecordFilter : IRecordFilter
    {
        public bool IsMatch(ITableRowReader reader, ITableRowValuesSelector selector) => true;
    }
}
