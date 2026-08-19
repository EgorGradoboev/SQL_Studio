using Npgsql;
using SQL_Studio.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Studio.ViewModels
{
    public class MainViewModel
    {
        public QueryTabViewModel QueryTabs { get; }
        public MainViewModel(NpgsqlConnection connection)
        {
            var executionService = new QueryExecutionService();
            QueryTabs = new QueryTabViewModel(connection, executionService);
        }
    }
}
