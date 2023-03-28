using System.Collections.Generic;
using System.Collections.Immutable;
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

        ValueTask<IReadOnlyList<string>> GetHeadersAsync();

        ValueTask<IReadOnlyList<QueryRecordRow>> QueryAsync(QueryContext queryContext, CancellationToken cancellationToken);
    }
}
