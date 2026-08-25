using Npgsql;
using SQL_Studio.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SQL_Studio.ViewModels
{
    public class MainViewModel
    {
        public QueryTabViewModel QueryTabs { get; }
        public ObservableCollection<ServerViewModel> Servers { get; } = new();
        private ConnectionFactoryService _connectionFactory;
        private string _databaseName;
        public MainViewModel(ConnectionFactoryService connectionFactory, string databaseName)
        {
            _connectionFactory = connectionFactory;
            _databaseName = databaseName;
            var executionService = new QueryExecutionService();
            QueryTabs = new QueryTabViewModel(_connectionFactory, executionService, _databaseName);
            Servers.Add(new ServerViewModel(_databaseName, QueryTabs, _connectionFactory));
        }
    }
}
