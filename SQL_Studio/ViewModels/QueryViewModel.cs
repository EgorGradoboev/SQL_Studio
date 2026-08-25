using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;
using Npgsql;
using SQL_Studio.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace SQL_Studio.ViewModels
{
    public class QueryViewModel : INotifyPropertyChanged
    {
        private readonly QueryExecutionService _executionService;
        private NpgsqlConnection? _connection;
        private CancellationTokenSource? _executionCts;
        private ConnectionFactoryService _connectionFactory;
        private string _queryText = "";
        private List<string>? _cachedTables;

        public string TabName { get; set; }
        public ICommand ExecuteCommand { get; }
        public ICommand CancelCommand { get; }
        public bool IsAutocompleteInsert { get; set; }
        public ObservableCollection<string> Suggestions { get; } = new();
        private bool _isAutoCompleteOpen;
        public bool IsAutoCompleteOpen
        {
            get => _isAutoCompleteOpen;
            set { _isAutoCompleteOpen = value; OnpropertyChanged(); }
        }
        public string QueryText
        {
            get => _queryText;
            set { _queryText = value; OnpropertyChanged(); }
        }
        private string _executionTimerText;
        private string _databaseName;
        public string ExecutionTimerText
        {
            get => _executionTimerText;
            set { _executionTimerText = value; OnpropertyChanged(); }
        }        
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

        public async Task ExecuteAsync()
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
        public async Task UpdateAutoComplete(int caretIndex)
        {
            if (IsAutocompleteInsert)
                return;
            _cachedTables ??= await GetTables();
            string currentWord = GetCurrentWord(QueryText, caretIndex);
            if (string.IsNullOrWhiteSpace(currentWord))
            {
                IsAutoCompleteOpen = false;
                return;
            }
            var matches = _cachedTables
                .Where(t => t.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .ToList();
            Suggestions.Clear();
            foreach (var m in matches)
            {                
                Suggestions.Add(m);
            }
            IsAutoCompleteOpen = matches.Count > 0;
        }
        public string GetCurrentWord(string text, int carretIndex)
        {
            if (string.IsNullOrEmpty(text) || carretIndex == 0)
                return "";

            int start = carretIndex - 1;

            while (start >= 0)
            {
                char c = text[start];
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '.')
                {
                    break;
                }

                start--;
            }

            int wordStart = start + 1;
            int wordLength = carretIndex - wordStart;

            return text.Substring(wordStart, wordLength);
        }
        public async Task<List<string>> GetTables()
        {
            NpgsqlConnection? connection = null;
            List<string> tables = new List<string>();
            try
            {
                connection = await _connectionFactory.OpenConnectionAsync(_databaseName);
                const string query = """
                    SELECT table_name
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                    ORDER BY table_name
                    """;
                await using var command = new NpgsqlCommand(query, connection);
                await using var reader = await command.ExecuteReaderAsync();                
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
                return tables;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to load tree of databases: {e.Message}");
                return tables;
            }
            finally
            {
                if (connection != null)
                {
                    await connection.CloseAsync();
                    await connection.DisposeAsync();
                }                
            }

        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnpropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
