using ListViewer.ConfiguresModel;

namespace ListViewer.Abstractions
{
    interface IDataQuerySourceFactory
    {
        IDataQuerySource Create(DataSource dataSource);
    }
}
