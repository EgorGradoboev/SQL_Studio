using Npgsql;
using SQL_Studio.Services;
using SQL_Studio.Services.Interfaces;
using SQL_Studio.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SQL_Studio.Tests
{
    public class QueryViewModelTests
    {
        public FakeQueryExecutionService fakeExecutionService = new();
        public FakeConnectionFactoryService fakeConnectionFactoryService = new();
        public FakeDialogService fakeDialogService = new();
        public ObservableCollection<HistoryQueries> fakeHistoryQueries = new();
        [Fact]
        public void GetCurrentWord_ReturnsWordBeforeCarret()
        {
            var vm = new QueryViewModel(fakeExecutionService, fakeConnectionFactoryService,
                "fakeDatabase", 0, fakeHistoryQueries, fakeDialogService);
            string text = "SELECT * FROM use";
            int caretIndex = 18;
            string result = vm.GetCurrentWord(text, caretIndex);
            Assert.Equal("use", result);
        }
        
    }
    public class FakeQueryExecutionService : IQueryExecutionService
    {
        public Task<QueryExecutionService.QueryExecutionResult> ExecuteAsync(
            NpgsqlConnection connection, string query, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
    public class FakeConnectionFactoryService : IConnectionFactoryService
    {
        public Task<NpgsqlConnection> OpenConnectionAsync(string fakeDatabase)
        {
            throw new NotImplementedException();
        }
    }
    public class FakeDialogService : IDialogService
    {
        public void ShowError(string fakeMessage)
        {
            throw new NotImplementedException();
        }
    }
}
