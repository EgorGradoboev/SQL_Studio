using Npgsql;
using SQL_Studio.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;
using System.Runtime.CompilerServices;

namespace SQL_Studio.ViewModels
{
    public class QueryViewModel : INotifyPropertyChanged
    {
        private readonly QueryExecutionService _executionService;
        private readonly NpgsqlConnection _connection;
        private CancellationTokenSource? _executionCts;

        public string QueryText { get; set; } = "";
        public string TabName { get; set; }
        private DataView? _queryResults;
        public DataView? QueryResults
        {
            get => _queryResults;
            set { _queryResults = value; OnpropertyChanged(); }
        }

        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnpropertyChanged(); }
        }
        public ICommand ExecuteCommand { get; }
        public ICommand CancelCommand { get; }
        public QueryViewModel(NpgsqlConnection connection, QueryExecutionService executionService, int counter)
        {
            _executionService = executionService;
            _connection = connection;
            TabName = $"Query {counter}";
            ExecuteCommand = new RelayCommand(async () => await ExecuteAsync());
            CancelCommand = new RelayCommand(async () => _executionCts?.Cancel());
        }

        private async Task ExecuteAsync()
        {
            _executionCts = new CancellationTokenSource();
            try
            {
                var result = await _executionService.ExecuteAsync(_connection, QueryText, _executionCts.Token);
                QueryResults = result.IsSelect ? result.ResultsView : null;
                StatusMessage = result.IsSelect
                    ? $"Rows returned: {result.ResultsView!.Count}"
                    : $"Command completed. Affected rows: {result.AffectedRows}";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Query cancelled";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to execute query: {ex.Message}";
            }
            finally
            {
                _executionCts.Dispose();
                _executionCts = null;
            }            
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnpropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
