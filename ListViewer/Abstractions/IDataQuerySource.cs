using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ListViewer.ConfiguresModel;
using ListViewer.Model;

namespace ListViewer.Abstractions
{
    interface IDataQuerySource
    {
        string ProviderName { get; }

        Task LoadAsync(DataSource dataSource);

        IEnumerable<QueryRecordRow> Query(QueryContext queryContext, CancellationToken cancellationToken);
    }
}
