using CommunityToolkit.Mvvm.Input;
using Npgsql;
using SQL_Studio.Services;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Windows.Documents;
using System.Windows.Input;

namespace SQL_Studio.ViewModels
{
    public class TableViewModel
    {
        public string TableName { get; }
        public int LimitRows { get; }
        public string DatabaseName { get; }
        public ICommand Select { get; }
        public ICommand Update { get; }
        public ICommand Insert { get; }
        public ICommand Delete { get; }
        private ConnectionFactoryService _connectionFactory;
        private readonly QueryTabViewModel _queryTabs;
        private List<string> _columns = new List<string>();
        public TableViewModel(string tableName, string databaseName, int limitRows, 
            QueryTabViewModel queryTabs,
            ConnectionFactoryService connectionFactory)
        {
            TableName = tableName;
            DatabaseName = databaseName;
            LimitRows = limitRows;
            _connectionFactory = connectionFactory;
            _queryTabs = queryTabs;
            Select = new RelayCommand(async () => await SelectTopRows());
            Update = new RelayCommand(async () => await UpdateRows());
            Insert = new RelayCommand(async () => await InsertRows());
            Delete = new RelayCommand(async () => await DeleteRows());
        }
        public async Task SelectTopRows()
        {
            string result = string.Join(", ", await GetColumnNames());
            string query = $"""
                SELECT {result}
                FROM {TableName}
                LIMIT {LimitRows}
                """;
            var tab = _queryTabs.NewTab();
            tab.QueryText = query;
            await tab.ExecuteAsync();
        }
        public async Task UpdateRows()
        {            
            string query = $"""
                -- Paste your data instead of *_*
                UPDATE {TableName}
                SET *column* = ''
                WHERE *_*;
                """;
            var tab = _queryTabs.NewTab();
            tab.QueryText = query;
        }
        public async Task InsertRows()
        {
            string result = string.Join(", ", await GetColumnNames());
            string query = $"""
                -- Paste your data instead of *_*
                INSERT INTO {TableName}({result})
                VALUES(*_*)
                """;
            var tab = _queryTabs.NewTab();
            tab.QueryText = query;
        }
        public async Task DeleteRows()
        {
            string result = string.Join(", ", await GetColumnNames());
            string query = $"""
                -- Paste your data instead of *_*
                DELETE FROM {TableName}
                WHERE *_*
                """;
            var tab = _queryTabs.NewTab();
            tab.QueryText = query;
        }
        private async Task<List<string>> GetColumnNames()
        {
            NpgsqlConnection? connection = null;
            try
            {
                connection = await _connectionFactory.OpenConnectionAsync(DatabaseName);
                _columns.Clear();
                string query = """
                SELECT column_name
                FROM information_schema.columns
                WHERE table_name = @table
                ORDER BY ordinal_position
                """;

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@table", TableName);
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    _columns.Add(reader.GetString(0));
                }
                return _columns;
            }
            catch (Exception)
            {              
                return _columns;
            }
            finally
            {
                if (connection != null)
                {
                    await connection.CloseAsync();
                    await connection.DisposeAsync();
                }                
            }
            
        }
    }
}
