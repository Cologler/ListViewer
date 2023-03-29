namespace ListViewer.ConfiguresModel
{
    interface ISqlite3DataSourceView : IDataSourceView
    {
        string? ConnectionString { get; }

        string? Table { get; }

        string GetConnectionString()
        {
            if (this.ConnectionString is not null)
                return this.ConnectionString;

            if ((this as DataSource)?.FilePath is { } path)
                return $"Data Source={path}";
            
            throw new BadConfigurationException("Cannot load data from a null ConnectionString.");
        }
    }
}
