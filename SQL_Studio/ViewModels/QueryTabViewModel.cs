using CommunityToolkit.Mvvm.Input;
using Npgsql;
using SQL_Studio.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace SQL_Studio.ViewModels
{
    public class QueryTabViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<QueryViewModel> Tabs { get; } = new();
        private QueryViewModel? _selectedTab;
        public QueryViewModel? SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnpropertyChanged(); }
        }
        private int _counter = 0;
        public ICommand NewTabCommand { get; set; }
        public ICommand CloseTabCommand { get; set; }
        private readonly QueryExecutionService _executionService;
        private readonly ConnectionFactoryService _connectionFactory;
        private readonly string _databaseName;
        public QueryTabViewModel(ConnectionFactoryService connectionFactory, 
            QueryExecutionService executionService, string databaseName)
        {
            _connectionFactory = connectionFactory;
            _executionService = executionService;
            _databaseName = databaseName;
            NewTabCommand = new RelayCommand(NewTab);
            CloseTabCommand = new RelayCommand(CloseTab);
        }

        private void NewTab()
        {
            _counter++;
            var tab = new QueryViewModel(_executionService, _connectionFactory, 
                _databaseName, _counter);
            Tabs.Add(tab);
            SelectedTab = tab;
        }
        private void CloseTab()
        {
            if (SelectedTab == null)
            {
                return;
            }
            _counter--;
            Tabs.Remove(SelectedTab);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnpropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
