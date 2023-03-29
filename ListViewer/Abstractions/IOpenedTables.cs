namespace ListViewer.Abstractions;

interface IOpenedTables : IDisposable
{
    IAsyncEnumerable<ITable> GetTablesAsync(CancellationToken cancellationToken);
}
