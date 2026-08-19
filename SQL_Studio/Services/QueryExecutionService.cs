using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SQL_Studio.Services
{
    public class QueryExecutionService
    {
        public class QueryExecutionResult
        {
            public DataView? ResultsView { get; init; }
            public int? AffectedRows { get; init; }
            public bool IsSelect => ResultsView != null;
            public static QueryExecutionResult ForSelect(DataTable table) => new()
            {
                ResultsView = table.DefaultView
            };
            public static QueryExecutionResult ForCommand(int affectedRows) => new()
            {
                AffectedRows = affectedRows
            };
        }
        public async Task<QueryExecutionResult> ExecuteAsync(NpgsqlConnection connection, string query, 
            CancellationToken cancellationToken)
        {
            using var command = new NpgsqlCommand(query, connection);
            if (query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                var table = new DataTable();
                table.Load(reader);

                return QueryExecutionResult.ForSelect(table);
            }
            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            return QueryExecutionResult.ForCommand(affectedRows);
        }        
    }
}
