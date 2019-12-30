using ListViewer.ConfiguresModel;

namespace ListViewer.Abstractions
{
    interface IDataQuerySourceFactory
    {
        IDataSourceLoader Create(DataSource dataSource);
    }
}
