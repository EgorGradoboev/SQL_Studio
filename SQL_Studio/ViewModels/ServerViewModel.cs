using Npgsql;
using SQL_Studio.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace SQL_Studio.ViewModels
{
    public class ServerViewModel
    {
        public string ServerName { get; set; }
        private readonly QueryTabViewModel _queryTabs;
        private readonly ConnectionFactoryService _connectionFactory;
        private NpgsqlConnection _connection;
        private IDialogService _dialogService;
        public ObservableCollection<DatabaseViewModel> Databases { get; } = new();
        public ServerViewModel(string serverName, QueryTabViewModel queryTabs,
            ConnectionFactoryService connectionFactory, IDialogService dialogService)
        {            
            ServerName = serverName;
            _connectionFactory = connectionFactory;
            _queryTabs = queryTabs;
            _dialogService = dialogService;
        }
        public async Task LoadDatabasesAsync()
        {
            try
            {
                _connection = await _connectionFactory.OpenConnectionAsync("postgres");
                const string query = """
                SELECT datname FROM pg_database WHERE datistemplate = false ORDER BY datname
                """;
                await using var command = new NpgsqlCommand(query, _connection);
                await using var reader = await command.ExecuteReaderAsync();
                Databases.Clear();
                while (await reader.ReadAsync())
                {
                    Databases.Add(new DatabaseViewModel(reader.GetString(0),
                        _queryTabs, _connectionFactory, _dialogService));
                }
            }
            catch (Exception e)
            {
                _dialogService.ShowError($"Failed to load servers: {e.Message}");
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

    }
}
