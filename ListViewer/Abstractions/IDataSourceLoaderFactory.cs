using ListViewer.ConfiguresModel;

namespace ListViewer.Abstractions
{
    interface IDataSourceLoaderFactory
    {
        IDataSourceLoader Create(DataSource dataSource);
    }
}
