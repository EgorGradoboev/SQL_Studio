using Npgsql;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SQL_Studio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var connectionWindow = new ConnectionWindow();
            bool? result = connectionWindow.ShowDialog();
            if (result != true)
            {
                Shutdown();
                return;
            }
            string serverName = connectionWindow.ServerName;
            string login = connectionWindow.Login;
            string password = connectionWindow.Password;
            NpgsqlConnection connection = connectionWindow.Connection;

            var mainWindow = new MainWindow(login, password, serverName, connection);
            ShutdownMode = ShutdownMode.OnLastWindowClose;
            mainWindow.Show();
        }
    }



}
