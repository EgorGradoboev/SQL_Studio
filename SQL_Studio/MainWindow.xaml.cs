using Npgsql;
using SQL_Studio.ViewModels;
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
        public MainWindow()
        {
            InitializeComponent();            
        }        
    }
}