using Npgsql;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SQL_Studio.Services.Interfaces;
using SQL_Studio.Services;

namespace SQL_Studio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var services = new ServiceCollection();

            services.AddSingleton<IQueryExecutionService, QueryExecutionService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddTransient<ConnectionWindow>();
            services.AddTransient<HistoryWindow>();

            _serviceProvider = services.BuildServiceProvider();

            var connectionWindow = _serviceProvider.GetRequiredService<ConnectionWindow>();
            var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
            var executionService = _serviceProvider.GetRequiredService<IQueryExecutionService>();
            bool? result = connectionWindow.ShowDialog();
            if (result != true)
            {
                Shutdown();
                return;
            }
            if (connectionWindow.ServerName == null || connectionWindow.Port == null ||
                connectionWindow.Login == null || connectionWindow.Password == null)
            {
                Shutdown();
                return;
            }
                
            IConnectionFactoryService connectionFactoryService = new ConnectionFactoryService(
                connectionWindow.ServerName, connectionWindow.Port, 
                connectionWindow.Login, connectionWindow.Password);

            string databaseName = "postgres";
            var mainWindow = new MainWindow(
                connectionFactoryService, dialogService, executionService,
                connectionWindow.ServerName, databaseName);
            
            mainWindow.Show();
            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
    }



}
