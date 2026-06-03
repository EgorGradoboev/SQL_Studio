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

            var tablesNode = new TreeViewItem
            {
                Header = "Tables",
                IsExpanded = true
            };

            foreach (string table in tables)
            {
                var tableItem = new TreeViewItem
                {
                    Header = table
                };

                var contextMenu = new ContextMenu();

                var selectMenuItem = new MenuItem
                {
                    Header = "Select top 100 rows"
                };
                var insertMenuItem = new MenuItem
                {
                    Header = "Insert row"
                };
                var updateMenuItem = new MenuItem
                {
                    Header = "Update top 100 rows"
                };

                selectMenuItem.Click += SelectMenuItem_Click;
                insertMenuItem.Click += InsertMenuItem_Click;
                updateMenuItem.Click += UpdateMenuItem_Click;

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
