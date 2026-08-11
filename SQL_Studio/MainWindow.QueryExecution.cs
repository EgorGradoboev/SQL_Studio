using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.CodeDom;

namespace SQL_Studio
{
    public partial class MainWindow
    {
        private async void Button_Execute(object sender, RoutedEventArgs e)
        {
            if (_isExecuting == true)
            {
                return;
            }            
            if (!_connected || _connection is null)
            {
                MessageBox.Show("Please connect to server first.");
                return;
            }
            var selectedTab = (TabItem)QueryTabs.SelectedItem;
            if (selectedTab is null)
            {
                MessageBox.Show("No one query tab is opened");
                return;
            }
            var selectedTextBox = (TextBox)selectedTab.Content;
            _isExecuting = true;
            string query = selectedTextBox.Text;

            try
            {
                Stopwatch executionTimer = new Stopwatch();
                executionTimer.Start();
                using var command = new NpgsqlCommand(query, _connection);
                _executionCts = new CancellationTokenSource();
                var cancellationToken = _executionCts.Token;

                if (query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                    var table = new DataTable();
                    table.Load(reader);

                    ResultsGrid.ItemsSource = table.DefaultView;
                    MessageTextBlock.Text = $"Rows returned: {table.Rows.Count}";
                }
                else
                {
                    var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

                    if (query.TrimStart().StartsWith("CREATE", StringComparison.OrdinalIgnoreCase)
                        && _databaseNodes.TryGetValue(_connection.Database, out var currentDatabaseItem))
                    {
                        await LoadTablesForDatabase(currentDatabaseItem);
                    }

                    ResultsGrid.ItemsSource = null;
                    MessageTextBlock.Text = $"Command completed successfully. Rows affected: {affectedRows}";
                }
                 
                AddQueryToBufer(query);
                executionTimer.Stop();                
                ExecutionTimeBlock.Text = $"Execution time: {executionTimer.ElapsedMilliseconds} ms";
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Query cancelled");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to execute query with error: {ex.Message}");
                return;
            }
            finally
            {
                if (_executionCts != null) _executionCts.Dispose();
                _isExecuting = false;
            }
        }

        private async void Button_Cancel(object sender, RoutedEventArgs e)
        {
            try
            {
                _executionCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                MessageBox.Show("Query is not running");
                return;
            }
        }
    }
}
