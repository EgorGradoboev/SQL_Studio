using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SQL_Studio
{
    public partial class MainWindow
    {
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
        }
        private async void Button_ChangeServer(object sender, RoutedEventArgs e)
        {
            if (_connection is not null)
            {
                await _connection.CloseAsync();
                _connection = null;
            }            
            ItemsTree.Items.Clear();
            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            var connectionWindow = new ConnectionWindow();
            connectionWindow.Owner = this;
            bool? result = connectionWindow.ShowDialog();
            if (result != true)
            {
                return;
            }
            _connection = connectionWindow.Connection;
            _serverName = connectionWindow.ServerName;
            _login = connectionWindow.Login;
            _password = connectionWindow.Password;

            _connected = true;
            await Load_Server();
            await Load_Databases();
        }

        private async void Button_Disconnect(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    await _connection.CloseAsync();
            //    _connected = false;
            //    ItemsTree.Items.Clear();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Failed to disconnect from {_databaseName} database: {ex.Message}");
            //    return;
            //}

            //MessageTextBlock.Text = $"Disconnected from {_databaseName} database";
        }
    }
}
