namespace ListViewer.ConfiguresModel
{
    interface IDataFileDataSourceView : IDataSourceView
    {
        string? FilePath { get; }

        string? Encoding { get; }

        string GetFilePath() => this.FilePath ?? throw new BadConfigurationException("Cannot load data from a null FilePath.");
    }
}
