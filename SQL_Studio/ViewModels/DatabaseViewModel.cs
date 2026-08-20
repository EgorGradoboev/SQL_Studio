using Npgsql;
using SQL_Studio.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace SQL_Studio.ViewModels
{
    public class DatabaseViewModel : INotifyPropertyChanged
    {
        public string DatabaseName { get; }
        private NpgsqlConnection _connection;
        private readonly ConnectionFactoryService _connectionFactory;
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
        public DatabaseViewModel(string databaseName,
            ConnectionFactoryService connectionFactory)
        {
            DatabaseName = databaseName;
            _connectionFactory = connectionFactory;
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
                    Tables.Add(new TableViewModel(reader.GetString(0), DatabaseName));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to load tree of databases: {e.Message}");
                return;
            }
            finally
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
            
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
