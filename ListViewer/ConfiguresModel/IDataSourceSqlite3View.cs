namespace ListViewer.ConfiguresModel
{
    interface IDataSourceSqlite3View : IDataSourceView
    {
        string? ConnectionString { get; }

        string? Table { get; }
    }
}
