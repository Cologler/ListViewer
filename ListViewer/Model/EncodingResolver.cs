using System;
using System.Text;
using ListViewer.Abstractions;

namespace ListViewer.Model
{
    class EncodingResolver : IEncodingResolver
    {
        public Encoding GetEncoding(string? encoding)
        {
            if (encoding != null)
            {
                try
                {
                    return Encoding.GetEncoding(encoding);
                }
                catch (ArgumentException)
                {
                    throw new BadConfigurationException($"unknown encoding {encoding}");
                }
            }

            return Encoding.UTF8;
        }
    }
}
