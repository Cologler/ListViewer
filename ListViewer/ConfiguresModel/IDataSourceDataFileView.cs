namespace ListViewer.ConfiguresModel
{
    interface IDataSourceDataFileView : IDataSourceView
    {
        string? FilePath { get; }

        string? Encoding { get; }
    }
}
