using CommunityToolkit.Mvvm.Input;
using Npgsql;
using SQL_Studio.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace SQL_Studio.ViewModels
{
    public class MainViewModel
    {
        public QueryTabViewModel QueryTabs { get; }
        public ObservableCollection<ServerViewModel> Servers { get; } = new();
        private ConnectionFactoryService _connectionFactory;
        private string _databaseName;
        private IDialogService _dialogService;
        public ObservableCollection<HistoryQueries> HistoryQueries { get; }
        public string FilePath { get; set; }
        public ICommand ShowHistoryCommand { get; }
        public MainViewModel(ConnectionFactoryService connectionFactory, string databaseName)
        {
            _connectionFactory = connectionFactory;
            _databaseName = databaseName;
            var executionService = new QueryExecutionService();
            _dialogService = new DialogService();
            HistoryQueries = LoadHistoryFromFile();
            HistoryQueries.CollectionChanged += (s, e) => SaveHistoryToFile();

            QueryTabs = new QueryTabViewModel(_connectionFactory, executionService, _databaseName, HistoryQueries, _dialogService);
            Servers.Add(new ServerViewModel(_databaseName, QueryTabs, _connectionFactory, _dialogService));
            ShowHistoryCommand = new RelayCommand(ShowHistory);            
        }
        private void SaveHistoryToFile()
        {
            string json = JsonSerializer.Serialize(HistoryQueries);
            File.WriteAllText(FilePath, json);
        }
        private  ObservableCollection<HistoryQueries> LoadHistoryFromFile()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = System.IO.Path.Combine(folder, "SQL_Studio");
            FilePath = System.IO.Path.Combine(appFolder, "history_quries.json");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            if (File.Exists(FilePath))
            {
                string historyQueriesJson = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<ObservableCollection<HistoryQueries>>(historyQueriesJson) ?? new ObservableCollection<HistoryQueries>();

            }
            else
            {
                return new ObservableCollection<HistoryQueries>();

            }
        }
        public void AddNewServer(ConnectionFactoryService connectionFactory, string databaseName)
        {
            Servers.Add(new ServerViewModel(databaseName, QueryTabs, connectionFactory, _dialogService));
        }
        public void ShowHistory()
        {            
            var historyWindow = new HistoryWindow();
            historyWindow.DataContext = this;
            historyWindow.Show();
        }
    }
}
