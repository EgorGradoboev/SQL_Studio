using Npgsql;
using System.Collections;
using System.Data;
using System.Reflection.Emit;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private bool _isAutocompleteInsert = false;
        private List<string> _tables = new();
        private readonly Dictionary<TabItem, QueryEditorContext> _queryEditors = new();
        public MainWindow()
        {
            InitializeComponent();
        }
        private class QueryEditorContext
        {
            public TabItem Tab { get; set; }
            public TextBox SqlTextBox { get; set; }
            public Popup AutocompletePopup { get; set; }
            public ListBox AutocompleteListBox { get; set; }
        }
    }
}