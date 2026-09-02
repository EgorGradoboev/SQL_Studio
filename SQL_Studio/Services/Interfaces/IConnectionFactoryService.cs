using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Studio.Services.Interfaces
{
    public interface IConnectionFactoryService
    {
        Task<NpgsqlConnection> OpenConnectionAsync(string database);
    }
}
