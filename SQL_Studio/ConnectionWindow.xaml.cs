using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.RightsManagement;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        public string Port { get; set; }
        public string FilePath { get; set; }
        public List<RecentConnection> RecentConnections { get; set; }
        public NpgsqlConnection? Connection { get; private set; }
        private void RecentConnectionsComboBox_SelectionChanged (object sender, RoutedEventArgs e)
        {
            if (RecentConnectionsComboBox.SelectedItem is not RecentConnection selected)
                return;

            ServerConnectionTextBox.Text = selected.ServerName;
            PortConnectionTextBox.Text = selected.Port;
            LoginTextBox.Text = selected.Login;
        }
        private async void Button_ConnectServer(object sender, RoutedEventArgs e)
        {
            ServerName = ServerConnectionTextBox.Text;
            Login = LoginTextBox.Text;
            Password = PasswordTextBox.Password;
            Port = PortConnectionTextBox.Text;
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Port))
            {
                MessageBox.Show("Login, password or port are empty");
                return;
            }
            if (string.IsNullOrEmpty(ServerName))
            {
                MessageBox.Show("Enter server name!");
                return;
            }
            var connectionString = $"Host={ServerName};Port={Port};Database=postgres;Username={Login};Password={Password};";

            try
            {
                Connection = new NpgsqlConnection(connectionString);
                await Connection.OpenAsync();                             
                

                RecentConnection item = new RecentConnection();
                item.ServerName = ServerName;
                item.Login = Login;
                item.Port = Port;
                bool exist = false;
                if (RecentConnections != null)
                {
                    foreach (var connection in RecentConnections)
                    {
                        if (connection.DisplayName == item.DisplayName)
                        {
                            exist = true;
                            break;
                        }
                    }
                    if (exist == false)
                    {
                        RecentConnections.Insert(0, item);
                    }                    
                }               

                string updatedJson = JsonSerializer.Serialize(RecentConnections);
                File.WriteAllText(FilePath, updatedJson);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to {ServerName} server: {ex.Message}");
            }
        }
        private async void Button_CancelConnect(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        public ConnectionWindow()
        {
            InitializeComponent();
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = System.IO.Path.Combine(folder, "SQL_Studio");
            FilePath = System.IO.Path.Combine(appFolder, "recent_connections.json");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            if (File.Exists(FilePath))
            {
                string recentConnectionsJson = File.ReadAllText(FilePath);
                RecentConnections = JsonSerializer.Deserialize<List<RecentConnection>>(recentConnectionsJson) ?? new List<RecentConnection>();
                RecentConnectionsComboBox.ItemsSource = RecentConnections;

            }
            else
            {
                RecentConnections = new List<RecentConnection>();
            }            
        }
    }    
}
