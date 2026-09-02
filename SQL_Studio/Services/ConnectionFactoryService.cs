using Npgsql;
using SQL_Studio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Studio.Services
{
    public class ConnectionFactoryService : IConnectionFactoryService
    {
        private readonly string _serverName, _port, _login, _password;
        public ConnectionFactoryService(string serverName, string port, string login, string password)
        {
            _serverName = serverName;
            _port = port;
            _login = login;
            _password = password;
        }
        public async Task<NpgsqlConnection> OpenConnectionAsync(string database)
        {
            var connectionString = $"Host={_serverName};Port={_port};Database={database};Username={_login};Password={_password};";
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
