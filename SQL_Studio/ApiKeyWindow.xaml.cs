using System.Windows;

namespace SQL_Studio
{
    public partial class ApiKeyWindow : Window
    {
        public string? ApiKey { get; private set; }

        public ApiKeyWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => ApiKeyPasswordBox.Focus();
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {
            string apiKey = ApiKeyPasswordBox.Password.Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("Enter the API key");
                return;
            }
            ApiKey = apiKey;
            DialogResult = true;
        }

        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
