namespace ListViewer.Model;

record QueryResult(IReadOnlyList<QueryRecordHeader> Headers, IReadOnlyList<QueryRecordRow> Rows)
{

}
