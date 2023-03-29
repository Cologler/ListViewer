using ListViewer.Abstractions;

namespace ListViewer.Model;

class OpenedTables : IOpenedTables
{
    private readonly List<ITable> _tables = new();

    public OpenedTables() { }

    public OpenedTables(ITable table) : this() => this.AddTable(table);

    public void AddTable(ITable table) => this._tables.Add(table);

    public void Dispose()
    {
        foreach (var table in this._tables.AsEnumerable().Reverse())
        {
            table.Dispose();
        }
        this._tables.Clear();
    }

    public IAsyncEnumerable<ITable> GetTablesAsync(CancellationToken cancellationToken) => _tables.ToAsyncEnumerable();
}
