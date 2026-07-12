using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace SQL_Studio
{
    public partial class MainWindow
    {
        private async Task Show_Tables()
        {
            var sqlQuery = """
                SELECT table_name
                FROM information_schema.tables
                WHERE table_schema = 'public'
                ORDER BY table_name
                """;

            await using var command = new NpgsqlCommand(sqlQuery, _connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                _tables.Add(reader.GetString(0));
            }

            ItemsTree.Items.Clear();

            // TreeView for tables
            var tablesNode = new TreeViewItem();
            tablesNode.Header = "Tables";
            tablesNode.IsExpanded = true;

            // Context menu for tables
            var mainContextMenu = new ContextMenu();

            // Menu item for context menu
            var refreshTablesItem = new MenuItem();
            refreshTablesItem.Header = "Refresh";
            refreshTablesItem.Click += RefreshTablesItem_Click;

            // Elements relations
            mainContextMenu.Items.Add(refreshTablesItem);
            tablesNode.ContextMenu = mainContextMenu;
            
            foreach (string table in _tables)
            {
                // Tree View for particular table
                var tableItem = new TreeViewItem();
                tableItem.Header = table;

                // Context menu for particular table
                var contextMenu = new ContextMenu();

                // Select for particular table
                var selectMenuItem = new MenuItem();
                selectMenuItem.Header = "Select top 100 rows";

                // Insert for particular table
                var insertMenuItem = new MenuItem();
                insertMenuItem.Header = "Insert row";

                // Update for particular table
                var updateMenuItem = new MenuItem();
                updateMenuItem.Header = "Update top 100 rows";

                // Binding Clicks for contect menu
                selectMenuItem.Click += SelectMenuItem_Click;
                insertMenuItem.Click += InsertMenuItem_Click;
                updateMenuItem.Click += UpdateMenuItem_Click;

                // Elements relations
                contextMenu.Items.Add(selectMenuItem);
                contextMenu.Items.Add(updateMenuItem);
                contextMenu.Items.Add(insertMenuItem);
                tableItem.ContextMenu = contextMenu;
                tablesNode.Items.Add(tableItem);
            }

            ItemsTree.Items.Add(tablesNode);
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
