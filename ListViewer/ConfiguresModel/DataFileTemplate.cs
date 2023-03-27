using System.Collections.Generic;

namespace ListViewer.ConfiguresModel
{
    record DataFileTemplate
    {
        public string? Provider { get; set; }

        public string? Encoding { get; set; }

        public Dictionary<string, string?>? FieldMap { get; set; }

        public bool LoadEntireTableToMemory { get; set; }

        public DataSource CreateDataSource(string filePath)
        {
            return new DataSource
            {
                Provider = this.Provider,
                Encoding = this.Encoding,
                FieldMap = this.FieldMap,
                LoadEntireTableToMemory = this.LoadEntireTableToMemory,
                FilePath = filePath
            };
        }
    }
}
