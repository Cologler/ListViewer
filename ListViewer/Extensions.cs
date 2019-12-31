using System;
using System.Collections.Generic;
using System.Text;

namespace ListViewer
{
    public static class Extensions
    {
        public static void AddFrom<TK, TV>(this Dictionary<TK, TV> dict, IReadOnlyDictionary<TK, TV> source) where TK : notnull
        {
            foreach (var (k, v) in source)
            {
                dict.Add(k, v);
            }
        }
    }
}
