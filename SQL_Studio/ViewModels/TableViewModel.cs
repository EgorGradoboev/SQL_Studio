using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Studio.ViewModels
{
    public class TableViewModel
    {
        public string TableName { get; }
        public string DatabaseName { get; }
        public TableViewModel(string tableName, string databaseName)
        {
            TableName = tableName;
            DatabaseName = databaseName;
        }
    }
}
