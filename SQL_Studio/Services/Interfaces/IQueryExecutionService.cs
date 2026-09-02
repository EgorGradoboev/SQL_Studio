using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using static SQL_Studio.Services.QueryExecutionService;

namespace SQL_Studio.Services.Interfaces
{
    public interface IQueryExecutionService
    {
        Task<QueryExecutionResult> ExecuteAsync(NpgsqlConnection connection, string query,
            CancellationToken cancellationToken);
    }
}
