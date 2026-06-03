using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SQL_Studio
{
    public partial class MainWindow
    {
        private async void Button_NewQuery(object sender, RoutedEventArgs e)
        {
            _queryCounter++;
            var tab = new TabItem
            {
                Header = $"Query {_queryCounter}"
            };

            var textBox = new TextBox
            {
                AcceptsReturn = true,
                Name = $"QueryTextBox_{_queryCounter}",
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            tab.Content = textBox;
            QueryTabs.Items.Add(tab);
            QueryTabs.SelectedItem = tab;
        }

        private void Button_CloseQuery(object sender, RoutedEventArgs e)
        {
            var selectedTab = (TabItem)QueryTabs.SelectedItem;
            if (selectedTab is not null)
            {
                QueryTabs.Items.Remove(selectedTab);
                _queryCounter--;
            }
        }
    }
}
