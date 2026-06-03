using Npgsql;
using System.Collections;
using System.Data;
using System.Reflection.Emit;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace SQL_Studio
{
    public partial class MainWindow : Window
    {
        private NpgsqlConnection? _connection;
        private bool _connected = false;
        private string _databaseName;
        private int _queryCounter = 0;  
        private List<string> _queryBufer = new();
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}