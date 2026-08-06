using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            await EnsureConnectedToDatabase((string)treeItem.Tag);

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

            await EnsureConnectedToDatabase((string)treeItem.Tag);

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

            await EnsureConnectedToDatabase((string)treeItem.Tag);

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
        private async void RefreshDatabaseItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var databaseItem = (TreeViewItem)contextMenu.PlacementTarget;
            await LoadTablesForDatabase(databaseItem);
        }
        private void InsertSelectedAutocomplete_DoubleClick(QueryEditorContext context)
        {
            var sqlTextBox = context.SqlTextBox;
            var popup = context.AutocompletePopup;
            var listBox = context.AutocompleteListBox;

            if (listBox.SelectedItem is not string selectedTable)
                return;

            string currentWord = GetCurrentWord(sqlTextBox.Text, sqlTextBox.CaretIndex);

            if (string.IsNullOrEmpty(currentWord))
                return;

            int caretIndex = sqlTextBox.CaretIndex;
            int startIndex = caretIndex - currentWord.Length;

            _isAutocompleteInsert = true;

            sqlTextBox.SelectionStart = startIndex;
            sqlTextBox.SelectionLength = currentWord.Length;
            sqlTextBox.SelectedText = selectedTable;

            sqlTextBox.CaretIndex = startIndex + selectedTable.Length;

            _isAutocompleteInsert = false;

            popup.IsOpen = false;
            sqlTextBox.Focus();
        }
        private void SqlTextBox_PreviewKeyDown(QueryEditorContext context, KeyEventArgs e)
        {
            var popup = context.AutocompletePopup;
            var listBox = context.AutocompleteListBox;

            if (!popup.IsOpen)
                return;

            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                InsertSelectedAutocomplete_DoubleClick(context);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                popup.IsOpen = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                if (listBox.SelectedIndex < listBox.Items.Count - 1)
                    listBox.SelectedIndex++;

                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (listBox.SelectedIndex > 0)
                    listBox.SelectedIndex--;

                e.Handled = true;
            }
        }
    }
}
