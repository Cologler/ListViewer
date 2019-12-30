namespace ListViewer.Abstractions
{
    interface IRecordFilter
    {
        bool IsMatch(IRecordSearchFieldValuesReader reader);
    }
}
