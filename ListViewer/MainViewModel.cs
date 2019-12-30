using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ListViewer.ConfiguresModel;
using ListViewer.Model;
using Microsoft.Extensions.DependencyInjection;

namespace ListViewer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly object _syncRoot = new object();
        private CancellationTokenSource? _lastUpdateToken;
        private string _searchText = string.Empty;
        private string _currentStatus = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public Task LoadAsync()
        {
            return this.UpdateItemsAsync(0);
        }

        private async Task UpdateItemsAsync(int delay)
        {
            var searchText = this._searchText;
            if (delay > 0)
            {
                await Task.Delay(delay);
            }
            if (searchText != this._searchText) return;

            var tokenSource = new CancellationTokenSource();

            lock (this._syncRoot)
            {
                this._lastUpdateToken?.Cancel();
                this._lastUpdateToken?.Dispose();
                this._lastUpdateToken = tokenSource;
            }

            var token = tokenSource.Token;

            var sw = Stopwatch.StartNew();
            
            try
            {
                this.CurrentStatus = "searching...";
                var rows = await App.ServiceProvider.GetRequiredService<DataQueryProvider>()
                    .QueryAsync(this._searchText, token);
                this.Items.Clear();
                this.CurrentStatus = $"finished at {(double)sw.ElapsedMilliseconds/1000}s";

                if (token.IsCancellationRequested) return;
                foreach (var item in rows)
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

        public class RowViewModel
        {
            private readonly QueryRecordRow _row;

            internal RowViewModel(QueryRecordRow row)
            {
                this._row = row;
            }

            public string this[int index] => this._row.ColumnValues[index];
        }
    }
}
