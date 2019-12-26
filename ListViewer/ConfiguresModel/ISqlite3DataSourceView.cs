namespace ListViewer.ConfiguresModel
{
    interface ISqlite3DataSourceView : IDataSourceView
    {
        string? ConnectionString { get; }

        string? Table { get; }

        string GetConnectionString() => this.ConnectionString
            ?? throw new BadConfigurationException("Cannot load data from a null ConnectionString.");

        string GetTable() => this.Table ?? throw new BadConfigurationException("Cannot load data from a null Table.");
    }
}
