using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SQL_Studio
{
    public partial class MainWindow
    {
        private async void Button_Connect(object sender, RoutedEventArgs e)
        {
            if (_connected)
            {
                MessageBox.Show($"Already connected to {_connection.Database} database");
                return;
            }
            _databaseName = ConnectionTextBox.Text;
            var connectionString = $"Host=localhost;Port=5432;Database={_databaseName};Username=postgres;Password=1234;";

            try
            {
                _connection = new NpgsqlConnection(connectionString);
                await _connection.OpenAsync();
                await Show_Tables();
                _connected = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to {_databaseName} database: {ex.Message}");
            }

            MessageTextBlock.Text = $"Connected to {_databaseName} database";            
        }

        private async void Button_Disconnect(object sender, RoutedEventArgs e)
        {
            try
            {
                await _connection.CloseAsync();
                _connected = false;
                ItemsTree.Items.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to disconnect from {_databaseName} database: {ex.Message}");
                return;
            }

            MessageTextBlock.Text = $"Disconnected from {_databaseName} database";
        }
    }
}
