using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SQL_Studio
{
    public partial class MainWindow
    {
        private void Button_HistoryQuery(object sender, RoutedEventArgs e)
        {
            Button_NewQuery(sender, e);
            var selectedTab = (TabItem)QueryTabs.SelectedItem;
            foreach (string query in _queryBufer)
            {
                var selectedTextBox = (TextBox)selectedTab.Content;
                selectedTextBox.Text += query + "\n\r";
            }

        }
        private void AddQueryToBufer(string query)
        {
            _queryBufer.Add(query);
        }
    }
}
