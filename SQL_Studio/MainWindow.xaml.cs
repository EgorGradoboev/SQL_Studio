using Npgsql;
using SQL_Studio.Services.Interfaces;
using SQL_Studio.ViewModels;
using System.Security.Cryptography.X509Certificates;
using System.Windows;

namespace SQL_Studio
{
    public partial class MainWindow : Window
    {
        private string? _databaseName;
        private string? _serverName;
        private string? _login;
        private string? _password;
        private string? _port;
        private MainViewModel? _mainViewModel;
        private IConnectionFactoryService _connectionFactory;
        private IDialogService _dialogService;
        private IQueryExecutionService _executionService;
        public MainWindow(
            IConnectionFactoryService connectionFactoryService,IDialogService dialogService, IQueryExecutionService executionService,
            string serverName, string databaseName)
        {
            _databaseName = databaseName;
            _connectionFactory = connectionFactoryService;
            _dialogService = dialogService;
            _executionService = executionService;
            _mainViewModel = new MainViewModel(
                _connectionFactory, _dialogService, _executionService, 
                _databaseName);            
            DataContext = _mainViewModel;
            InitializeComponent();            
        }        
    }
}