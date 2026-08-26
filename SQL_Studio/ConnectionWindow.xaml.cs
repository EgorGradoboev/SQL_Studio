using Npgsql;
using System.IO;
using System.Text.Json;
using System.Windows;

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
        private NpgsqlConnection? _connection;
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
                _connection = new NpgsqlConnection(connectionString);
                await _connection.OpenAsync();                             
                

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
            finally
            {
                if (_connection != null)
                {
                    await _connection.CloseAsync();
                    _connection.Dispose();
                }                
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
