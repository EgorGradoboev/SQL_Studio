using CommunityToolkit.Mvvm.Input;
using Npgsql;
using SQL_Studio.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SQL_Studio.ViewModels
{
    public class DatabaseViewModel : INotifyPropertyChanged
    {
        public string DatabaseName { get; }
        public ICommand Refresh { get; }
        private int _limitRows = 100;
        private NpgsqlConnection? _connection;
        private readonly QueryTabViewModel _queryTabs;
        private readonly ConnectionFactoryService _connectionFactory;
        private readonly IDialogService _dialogService;
        public ObservableCollection<TableViewModel> Tables { get; } = new();
        private bool _tablesLoaded;
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
                if (value && !_tablesLoaded)
                {
                    _tablesLoaded = true;
                    _ = LoadTablesAsync();
                }
            }
        }
        public DatabaseViewModel(string databaseName, QueryTabViewModel queryTabs,
            ConnectionFactoryService connectionFactory, IDialogService dialogService)
        {
            DatabaseName = databaseName;
            _connectionFactory = connectionFactory;
            _queryTabs = queryTabs;
            _dialogService = dialogService;
            Refresh = new RelayCommand(RefreshTables);
        }
        public async void RefreshTables()
        {
            Tables.Clear();
            await LoadTablesAsync();
        }
        public async Task LoadTablesAsync()
        {
            try
            {
                _connection = await _connectionFactory.OpenConnectionAsync(DatabaseName);
                const string query = """
                    SELECT table_name
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                    ORDER BY table_name
                    """;
                await using var command = new NpgsqlCommand(query, _connection);
                await using var reader = await command.ExecuteReaderAsync();
                Tables.Clear();
                while (await reader.ReadAsync())
                {
                    Tables.Add(new TableViewModel(reader.GetString(0), DatabaseName, 
                        _limitRows, _queryTabs, _connectionFactory, _dialogService));
                }
            }
            catch (Exception e)
            {
                _dialogService.ShowError($"Failed to load tree of databases: {e.Message}");
                return;
            }
            finally
            {
                if (_connection != null)
                {
                    await _connection.CloseAsync();
                    await _connection.DisposeAsync();
                }                
            }
            
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
