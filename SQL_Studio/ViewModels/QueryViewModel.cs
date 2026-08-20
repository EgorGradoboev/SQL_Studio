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
using System.Diagnostics;

namespace SQL_Studio.ViewModels
{
    public class QueryViewModel : INotifyPropertyChanged
    {
        private readonly QueryExecutionService _executionService;
        private NpgsqlConnection? _connection;
        private CancellationTokenSource? _executionCts;
        private ConnectionFactoryService _connectionFactory;

        public string QueryText { get; set; } = "";
        private string _executionTimerText;
        private string _databaseName;
        public string ExecutionTimerText
        {
            get => _executionTimerText;
            set { _executionTimerText = value; OnpropertyChanged(); }
        }
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
        public QueryViewModel(QueryExecutionService executionService, 
            ConnectionFactoryService connectionFactory, string databaseName,
            int counter)
        {
            _databaseName = databaseName;
            _connectionFactory = connectionFactory;
            _executionService = executionService;
            TabName = $"Query {counter}";            
            ExecuteCommand = new RelayCommand(async () => await ExecuteAsync());
            CancelCommand = new RelayCommand(async () => _executionCts?.Cancel());
        }
        private async Task<NpgsqlConnection> GetConnectionAsync()
        {
            if (_connection is null)
            {
                _connection = await _connectionFactory.OpenConnectionAsync(_databaseName);
            }
            return _connection;
        }

        private async Task ExecuteAsync()
        {
            ExecutionTimerText = "Executing...";
            _executionCts = new CancellationTokenSource();
            Stopwatch executionTimer = new Stopwatch();
            executionTimer.Start();
            try
            {
                var connection = await GetConnectionAsync();
                var result = await _executionService.ExecuteAsync(connection, QueryText, _executionCts.Token);
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
                executionTimer.Stop();
                ExecutionTimerText = $"Execution time: {executionTimer.ElapsedMilliseconds}";
            }            
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnpropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
