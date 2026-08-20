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
        private readonly ConnectionFactoryService _connectionFactory;
        private NpgsqlConnection _connection;
        public ObservableCollection<DatabaseViewModel> Databases { get; } = new();
        public ServerViewModel(string serverName, ConnectionFactoryService connectionFactory)
        {
            ServerName = serverName;
            _connectionFactory = connectionFactory;
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
                    Databases.Add(new DatabaseViewModel(reader.GetString(0), _connectionFactory));
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

    }
}
