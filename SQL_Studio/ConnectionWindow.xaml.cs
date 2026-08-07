using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SQL_Studio
{
    /// <summary>
    /// Interaction logic for ConnectionWindow.xaml
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        public string ServerName { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }
        public NpgsqlConnection? Connection { get; private set; }
        private async void Button_ConnectServer(object sender, RoutedEventArgs e)
        {
            ServerName = ServerConnectionTextBox.Text;
            Login = LoginTextBox.Text;
            Password = PasswordTextBox.Password;
            var connectionString = $"Host={ServerName};Port=5432;Database=postgres;Username={Login};Password={Password};";

            try
            {
                Connection = new NpgsqlConnection(connectionString);
                await Connection.OpenAsync();
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to {ServerName} server: {ex.Message}");
            }
        }
        private async void Button_CancelConnect(object sender, RoutedEventArgs e)
        {
            return;
        }
        public ConnectionWindow()
        {
            InitializeComponent();            
        }
    }
}
