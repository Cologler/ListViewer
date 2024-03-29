﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ListViewer.Abstractions;
using ListViewer.ConfiguresModel;
using Microsoft.Extensions.DependencyInjection;

namespace ListViewer.Model
{
    class DirectoryDataSourceLoader: IDataSourceLoader
    {
        private readonly CollectionDataSourceLoader _dataSourceLoader;

        public DirectoryDataSourceLoader(CollectionDataSourceLoader dataSourceLoader)
        {
            this._dataSourceLoader = dataSourceLoader;
        }

        public string ProviderName => DataProviderNames.Directory;

        static IEnumerable<string> EnumerableFiles(string dirPath, int depth)
        {
            static void Walk(List<string> results, string dirPath, int depth)
            {
                results.AddRange(Directory.GetFiles(dirPath));
                if (depth > 0)
                {
                    foreach (var subDirPath in Directory.GetDirectories(dirPath))
                    {
                        Walk(results, subDirPath, depth - 1);
                    }
                }
            }

            var r = new List<string>();
            Walk(r, dirPath, depth);
            return r;
        }

        public async Task ConfigureAsync(DataSource dataSource)
        {
            var dataSourceView = (IDirectoryDataSourceView)dataSource;
            var templates = dataSourceView.Templates;
            var dirPath = dataSourceView.DirPath ?? throw new BadConfigurationException("DirPath can not be null.");

            if (templates != null)
            {
                foreach (var path in EnumerableFiles(dirPath, dataSourceView.SubDirDepth ?? 0))
                {
                    if (Path.GetExtension(path) is string ext && templates.TryGetValue(ext, out var value) && value is DataFileTemplate dft)
                    {
                        await this._dataSourceLoader.AddDataSourceAsync(dft.CreateDataSource(path));
                    }
                }
            }
        }

        public ValueTask<IReadOnlyList<QueryRecords>> QueryAsync(QueryContext queryContext, CancellationToken cancellationToken)
            => this._dataSourceLoader.QueryAsync(queryContext, cancellationToken);
    }
}
