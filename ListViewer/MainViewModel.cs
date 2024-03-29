﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

using ListViewer.ConfiguresModel;
using ListViewer.Model;
using Microsoft.Extensions.DependencyInjection;

namespace ListViewer
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _serviceProvider;
        private CancellationTokenSource? _lastUpdateToken;
        private string _searchText = string.Empty;
        private string _currentStatus = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(IServiceProvider serviceProvider, DataQueryProvider dataQueryProvider)
        {
            this._serviceProvider = serviceProvider;
            this.DataQueryProvider = dataQueryProvider;
        }

        public string SearchText
        {
            get => this._searchText;
            set
            {
                if (this._searchText != value)
                {
                    this._searchText = value;
                    _ = this.UpdateItemsAsync(300);
                }
            }
        }

        internal void Cancel()
        {
            this._lastUpdateToken?.Cancel();
            this._lastUpdateToken?.Dispose();
        }

        public string CurrentStatus
        {
            get => this._currentStatus;
            set
            {
                if (this._currentStatus != value)
                {
                    this._currentStatus = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentStatus)));
                }
            }
        }

        public Task LoadAsync() => this.UpdateItemsAsync(0);

        private async Task UpdateItemsAsync(int delay)
        {
            var searchText = this._searchText?.Trim() ?? string.Empty;
            if (delay > 0)
            {
                await Task.Delay(delay);
            }
            if (searchText != this._searchText) return;

            var tokenSource = new CancellationTokenSource();

            this.Cancel();
            this._lastUpdateToken = tokenSource;

            var token = tokenSource.Token;

            var sw = Stopwatch.StartNew();
            
            try
            {
                this.CurrentStatus = "searching...";
                var records = await Task.Run(() => this.DataQueryProvider.QueryAsync(searchText, token), token);
                this.Items.Clear();
                this.CurrentStatus = $"found {records.Rows.Count} rows, finished at {(double)sw.ElapsedMilliseconds/1000}s";

                if (token.IsCancellationRequested) return;

                var viewColumns = this.GridViewColumns!;
                viewColumns.Clear();
                var columns = records.Headers
                    .Select((z, i) =>
                    {
                        return new GridViewColumn
                        {
                            Header = z.Name,
                            DisplayMemberBinding = new Binding($"[{i}]")
                        };
                    });
                foreach (var column in columns)
                {
                    viewColumns.Add(column);
                }

                foreach (var item in records.Rows)
                {
                    this.Items.Add(new RowViewModel(item));
                }
            }
            finally
            {
                sw.Stop();
            }
        }

        public ObservableCollection<RowViewModel> Items { get; } = new ObservableCollection<RowViewModel>();

        public DataQueryProvider DataQueryProvider { get; }

        public GridViewColumnCollection? GridViewColumns { get; set; }

        public class RowViewModel
        {
            private readonly QueryRecordRow _row;

            internal RowViewModel(QueryRecordRow row)
            {
                this._row = row;
            }

            public string this[int index] => this._row.GetColumnValue(index);
        }
    }
}
