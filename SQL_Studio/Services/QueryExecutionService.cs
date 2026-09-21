using Npgsql;
using SQL_Studio.Services.Interfaces;
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
    public class QueryExecutionService : IQueryExecutionService
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
            if (ReturnsRows(query))
            {
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                var table = new DataTable();
                table.Load(reader);

                return QueryExecutionResult.ForSelect(table);
            }
            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            return QueryExecutionResult.ForCommand(affectedRows);
        }

        private static bool ReturnsRows(string query)
        {
            int i = 0;
            while (i < query.Length)
            {
                if (char.IsWhiteSpace(query[i]))
                {
                    i++;
                }
                else if (query.AsSpan(i).StartsWith("--"))
                {
                    int lineEnd = query.IndexOf('\n', i);
                    i = lineEnd < 0 ? query.Length : lineEnd + 1;
                }
                else if (query.AsSpan(i).StartsWith("/*"))
                {
                    int blockEnd = query.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    i = blockEnd < 0 ? query.Length : blockEnd + 2;
                }
                else
                {
                    break;
                }
            }
            var rest = query.AsSpan(i);
            return rest.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
                || rest.StartsWith("WITH", StringComparison.OrdinalIgnoreCase);
        }
    }
}
