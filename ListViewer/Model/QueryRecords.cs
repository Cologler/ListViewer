using System.Collections.Immutable;

namespace ListViewer.Model;

record QueryRecords(ImmutableArray<string> Headers, IReadOnlyList<QueryRecordRow> Rows)
{

}
