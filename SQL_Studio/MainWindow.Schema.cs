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

            var tables = new List<string>();
            await using var command = new NpgsqlCommand(sqlQuery, _connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
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
            
            foreach (string table in tables)
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
    }
}
