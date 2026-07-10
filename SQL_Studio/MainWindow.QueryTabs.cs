using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SQL_Studio
{
    public partial class MainWindow
    {
        private async void Button_NewQuery(object sender, RoutedEventArgs e)
        {
            _queryCounter++;

            // Qury tab
            var tab = new TabItem();
            tab.Name = $"Query_{_queryCounter}";

            // Query textbox
            var textBox = new TextBox();
            textBox.AcceptsReturn = true;
            textBox.Name = $"QueryTextBox_{_queryCounter}";
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            // Query popup
            var popup = new Popup();
            popup.Name = "AutoCompletePopup";
            popup.StaysOpen = false;
            popup.AllowsTransparency = false;
            popup.PlacementTarget = textBox;
            popup.Placement = PlacementMode.Relative;

            // Popup border
            var popupBorder = new Border();
            popupBorder.Name = "PopupBorder";
            popupBorder.Background = System.Windows.Media.Brushes.White;
            popupBorder.BorderBrush = System.Windows.Media.Brushes.Black;
            popupBorder.BorderThickness = new Thickness(1);
            popupBorder.MinWidth = 200;
            popupBorder.MaxHeight = 200;

            // Popup listbox
            var popupListBox = new ListBox();
            popup.Name = "PopupListBox";
            popupListBox.MouseDoubleClick += PopupListBox_MouseDoubleClick;

            // Elements relations
            popupBorder.Child = popupListBox;
            popup.Child = popupBorder;
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
