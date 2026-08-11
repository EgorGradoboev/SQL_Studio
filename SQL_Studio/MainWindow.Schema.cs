using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SQL_Studio
{
    public partial class MainWindow
    {
        private async Task Load_Server()
        {
            _serverTreeView.Header = _serverName;
            _serverTreeView.IsExpanded = true;
            ItemsTree.Items.Add(_serverTreeView);
        }
        private async Task Load_Databases()
        {
            if (_connection is null)
                return;

            const string query = """
                SELECT datname FROM pg_database WHERE datistemplate = false ORDER BY datname
                """;

            _databases.Clear();
            {
                await using var command = new NpgsqlCommand(query, _connection);
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    _databases.Add(reader.GetString(0));
                }
            }

            _serverTreeView.Items.Clear();
            _databasesTreeView.Items.Clear();
            _databaseNodes.Clear();

            //TreeView for databases
            _databasesTreeView.Header = "Databases";
            _databasesTreeView.IsExpanded = true;

            foreach (string database in _databases)
            {
                var databaseItem = new TreeViewItem();
                databaseItem.Header = database;
                databaseItem.Tag = database;

                // Placeholder so the node shows an expand arrow before tables are loaded
                databaseItem.Items.Add(new TreeViewItem { Header = "Loading..." });
                databaseItem.Expanded += DatabaseItem_Expanded;

                var contextMenu = new ContextMenu();
                var refreshItem = new MenuItem { Header = "Refresh" };
                refreshItem.Click += RefreshDatabaseItem_Click;
                contextMenu.Items.Add(refreshItem);
                databaseItem.ContextMenu = contextMenu;

                _databaseNodes[database] = databaseItem;
                _databasesTreeView.Items.Add(databaseItem);
            }

            _serverTreeView.Items.Add(_databasesTreeView);

            // Eagerly load tables for the database we're actually connected to
            if (_databaseNodes.TryGetValue(_connection.Database, out var currentDatabaseItem))
            {
                await LoadTablesForDatabase(currentDatabaseItem);
                currentDatabaseItem.IsExpanded = true;
            }
        }

        private async void DatabaseItem_Expanded(object sender, RoutedEventArgs e)
        {
            // Expanded bubbles up the tree - ignore bubbled events from children
            if (e.OriginalSource != sender)
                return;

            var databaseItem = (TreeViewItem)sender;
            bool notLoadedYet = databaseItem.Items.Count == 1
                && databaseItem.Items[0] is TreeViewItem placeholder
                && (string)placeholder.Header == "Loading...";

            if (notLoadedYet)
            {
                await LoadTablesForDatabase(databaseItem);
            }
        }

        private async Task LoadTablesForDatabase(TreeViewItem databaseItem)
        {
            if (_connection is null)
                return;

            string databaseName = (string)databaseItem.Tag;
            bool isActiveDatabase = databaseName == _connection.Database;

            // Postgres connections are bound to a single database, so tables of
            // any other database need their own short-lived connection
            NpgsqlConnection connection = _connection;
            NpgsqlConnection? tempConnection = null;
            if (!isActiveDatabase)
            {
                var connectionString = $"Host={_serverName};Port=5432;Database={databaseName};Username={_login};Password={_password}";
                tempConnection = new NpgsqlConnection(connectionString);
                await tempConnection.OpenAsync();
                connection = tempConnection;
            }

            const string sqlQuery = """
                SELECT table_name
                FROM information_schema.tables
                WHERE table_schema = 'public'
                ORDER BY table_name
                """;

            var tables = new List<string>();
            try
            {
                await using var command = new NpgsqlCommand(sqlQuery, connection);
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            finally
            {
                if (tempConnection != null)
                    await tempConnection.CloseAsync();
            }

            databaseItem.Items.Clear();
            foreach (string table in tables)
            {
                databaseItem.Items.Add(CreateTableTreeViewItem(table, databaseName));
            }

            if (isActiveDatabase)
            {
                _tables = tables;
            }
        }

        private async Task EnsureConnectedToDatabase(string databaseName)
        {
            if (_connection is null || _connection.Database == databaseName)
                return;

            await _connection.CloseAsync();
            var connectionString = $"Host={_serverName};Port=5432;Database={databaseName};Username={_login};Password={_password};";
            _connection = new NpgsqlConnection(connectionString);
            await _connection.OpenAsync();
            _databaseName = databaseName;

            // Keep autocomplete in sync with whichever database is now active
            if (_databaseNodes.TryGetValue(databaseName, out var databaseItem))
            {
                _tables = databaseItem.Items
                    .Cast<TreeViewItem>()
                    .Select(item => (string)item.Header)
                    .ToList();
            }
        }

        private TreeViewItem CreateTableTreeViewItem(string table, string databaseName)
        {
            var tableItem = new TreeViewItem();
            tableItem.Header = table;
            tableItem.Tag = databaseName;

            var contextMenu = new ContextMenu();

            var selectMenuItem = new MenuItem { Header = "Select top 100 rows" };
            var insertMenuItem = new MenuItem { Header = "Insert row" };
            var updateMenuItem = new MenuItem { Header = "Update top 100 rows" };

            selectMenuItem.Click += SelectMenuItem_Click;
            insertMenuItem.Click += InsertMenuItem_Click;
            updateMenuItem.Click += UpdateMenuItem_Click;

            contextMenu.Items.Add(selectMenuItem);
            contextMenu.Items.Add(updateMenuItem);
            contextMenu.Items.Add(insertMenuItem);
            tableItem.ContextMenu = contextMenu;

            return tableItem;
        }
        private async Task<List<string>> GetColumnNames(string table)
        {

            const string query = """
                SELECT column_name
                FROM information_schema.columns
                WHERE table_name = @table
                ORDER BY ordinal_position
                """;

            await using var command = new NpgsqlCommand(query, _connection);

            command.Parameters.AddWithValue("@table", table);

            await using var reader = await command.ExecuteReaderAsync();
            var columns = new List<string>();
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(0));
            }
            return columns;
        }
        private string GetCurrentWord (string text, int carretIndex)
        {
            if (string.IsNullOrEmpty(text) || carretIndex == 0)
                return "";

            int start = carretIndex - 1;

            while (start >= 0)
            {
                char c = text[start];
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '.')
                {
                    break;
                }

                start--;
            }

            int wordStart = start + 1;
            int wordLength = carretIndex - wordStart;

            return text.Substring(wordStart, wordLength);
        }
        private void ShowAutoComplete(QueryEditorContext context)
        {
            if (_isAutocompleteInsert)
                return;
            var sqlTextBox = context.SqlTextBox;
            var popup = context.AutocompletePopup;
            var listbox = context.AutocompleteListBox;

            string currentWord = GetCurrentWord(sqlTextBox.Text, sqlTextBox.CaretIndex);            
            if (string.IsNullOrWhiteSpace(currentWord))
            {
                popup.IsOpen = false;
                return;
            }

            var matches = _tables
                .Where(t => t.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .ToList();
            if (matches.Count == 0)
            {
                popup.IsOpen = false;
                return;
            }

            listbox.ItemsSource = matches;
            listbox.SelectedIndex = 0;

            var rect = sqlTextBox.GetRectFromCharacterIndex(sqlTextBox.CaretIndex);

            popup.HorizontalOffset = rect.X;
            popup.VerticalOffset = rect.Y + rect.Height;

            popup.IsOpen = true;
        }
    }
}
