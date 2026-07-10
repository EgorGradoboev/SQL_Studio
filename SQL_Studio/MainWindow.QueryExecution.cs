using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace SQL_Studio
{
    public partial class MainWindow
    {
        private async void Button_Execute(object sender, RoutedEventArgs e)
        {
            if (!_connected)
            {
                MessageBox.Show("Please connect to server first.");
                return;
            }
            var selectedTab = (TabItem)QueryTabs.SelectedItem;
            var selectedTextBox = (TextBox)selectedTab.Content;

            string query = selectedTextBox.Text;

            try
            {
                Stopwatch executionTimer = new Stopwatch();
                executionTimer.Start();
                using var command = new NpgsqlCommand(query, _connection);

                if (query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    await using var reader = await command.ExecuteReaderAsync();

                    var table = new DataTable();
                    table.Load(reader);

                    ResultsGrid.ItemsSource = table.DefaultView;
                    MessageTextBlock.Text = $"Rows returned: {table.Rows.Count}";
                }
                else
                {
                    if (query.TrimStart().StartsWith("CREATE", StringComparison.OrdinalIgnoreCase))
                    {
                        await Show_Tables();
                    }

                    var affectedRows = await command.ExecuteNonQueryAsync();

                    ResultsGrid.ItemsSource = null;
                    MessageTextBlock.Text = $"Command completed successfully. Rows affected: {affectedRows}";
                }
                AddQueryToBufer(query);
                executionTimer.Stop();                
                ExecutionTimeBlock.Text = $"Execution time: {executionTimer.ElapsedMilliseconds} ms";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to execute query with error: {ex.Message}");
                return;
            }
        }
    }
}
