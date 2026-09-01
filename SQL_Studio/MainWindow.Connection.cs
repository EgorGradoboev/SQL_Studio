using SQL_Studio.Services;
using SQL_Studio.ViewModels;
using System.Windows;

namespace SQL_Studio
{
    public partial class MainWindow
    {
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!await ConnectToServer()) return;
            ConnectionFactoryService connectionFactory =
                new ConnectionFactoryService(_serverName, _port, _login, _password);

            _mainViewModel = new MainViewModel(connectionFactory, _databaseName);
            DataContext = _mainViewModel;
            foreach (var server in _mainViewModel.Servers)
            {
                await server.LoadDatabasesAsync();
            }
        }
        private async void Button_ChangeServer(object sender, RoutedEventArgs e)
        {
            if (!await ConnectToServer()) return;
            ConnectionFactoryService connectionFactory =
                new ConnectionFactoryService(_serverName, _port, _login, _password);
            _mainViewModel.AddNewServer(connectionFactory, _databaseName);
            var server = _mainViewModel.Servers.Last();
            await server.LoadDatabasesAsync();
        }
        private async Task<bool> ConnectToServer()
        {
            var connectionWindow = new ConnectionWindow();
            connectionWindow.Owner = this;
            bool? result = connectionWindow.ShowDialog();
            if (result != true)
            {
                return false;
            }
            _serverName = connectionWindow.ServerName;
            _login = connectionWindow.Login;
            _password = connectionWindow.Password;
            _port = connectionWindow.Port;
            _databaseName = "postgres";
            return true;
        }

        
    }
}
