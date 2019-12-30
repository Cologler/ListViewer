using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ListViewer.ConfiguresModel;
using ListViewer.Model;

namespace ListViewer.Abstractions
{
    interface IDataSourceLoader
    {
        string ProviderName { get; }

        Task ConfigureAsync(DataSource dataSource);

        IEnumerable<QueryRecordRow> Query(QueryContext queryContext, CancellationToken cancellationToken);
    }
}
