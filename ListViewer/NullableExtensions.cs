using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ListViewer
{
    public static class NullableExtensions
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> o) where T : class
        {
            return o.Where(x => x != null)!;
        }
    }
}
