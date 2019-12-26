using System;
using System.Collections.Generic;
using System.Text;

namespace ListViewer.Abstractions
{
    interface IEncodingResolver
    {
        Encoding GetEncoding(string? encoding);
    }
}
