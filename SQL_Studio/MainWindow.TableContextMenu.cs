using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SQL_Studio
{
    public partial class MainWindow
    {
        private async Task<DataTable> GetTableData(string table)
        {
            string query = $"""
                SELECT * 
                FROM {table} 
                LIMIT 100
                """;
            using var command = new NpgsqlCommand(query, _connection);
            await using var reader = await command.ExecuteReaderAsync();
            var dataTable = new DataTable();
            dataTable.Load(reader);
            return dataTable;
        }

        private async void SelectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var treeItem = (TreeViewItem)contextMenu.PlacementTarget;
            string table = treeItem.Header.ToString();

            var dataTable = await GetTableData(table);
            List<string> columnNames = await GetColumnNames(table);
            string result = string.Join(", ", columnNames);

            ResultsGrid.ItemsSource = dataTable.DefaultView;
            MessageTextBlock.Text = $"Rows returned: {dataTable.Rows.Count}";

            Button_NewQuery(sender, e);

            var selectedTab = (TabItem)QueryTabs.SelectedItem;
            var selectedTextBox = (TextBox)selectedTab.Content;

            string query = $"""
                SELECT {result}
                FROM {table}
                LIMIT 100
                """;

            selectedTextBox.Text = query;
        }

        private async void UpdateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var treeItem = (TreeViewItem)contextMenu.PlacementTarget;
            string table = treeItem.Header.ToString();

            var dataTable = await GetTableData(table);
            ResultsGrid.ItemsSource = dataTable.DefaultView;
            MessageTextBlock.Text = $"Rows returned: {dataTable.Rows.Count}";

            string query = $"""
                -- Paste your data instead of *_*
                UPDATE {table}
                SET *column* = ''
                WHERE ;
                """;
            Button_NewQuery(sender, e);
            var selectedTab = (TabItem)QueryTabs.SelectedItem;
            var selectedTextBox = (TextBox)selectedTab.Content;
            selectedTextBox.Text = query;
        }

        private async void InsertMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var treeItem = (TreeViewItem)contextMenu.PlacementTarget;
            string table = treeItem.Header.ToString();

            var dataTable = await GetTableData(table);
            ResultsGrid.ItemsSource = dataTable.DefaultView;
            MessageTextBlock.Text = $"Rows returned: {dataTable.Rows.Count}";

            List<string> columnNames = await GetColumnNames(table);
            string result = string.Join(", ", columnNames);

            string query = $"""
                -- Paste your data instead of *_*
                INSERT INTO {table}({result})
                VALUES()
                """;
            Button_NewQuery(sender, e);

            var selectedTab = (TabItem)QueryTabs.SelectedItem;
            var selectedTextBox = (TextBox)selectedTab.Content;
            selectedTextBox.Text = query;
        }
        private async void RefreshTablesItem_Click(object sender, RoutedEventArgs e)
        {
            Show_Tables();
        }
        private async void PopupListBox_MouseDoubleClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
