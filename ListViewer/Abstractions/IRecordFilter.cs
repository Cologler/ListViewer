namespace ListViewer.Abstractions
{
    interface IRecordFilter
    {
        bool IsMatch(ITableRowReader reader, ITableRowValuesSelector selector);
    }
}
