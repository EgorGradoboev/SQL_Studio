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
            tab.Header = $"Query_{_queryCounter}";

            // Query textbox
            var sqlTextBox = new TextBox();
            sqlTextBox.AcceptsReturn = true;
            sqlTextBox.Name = $"QueryTextBox_{_queryCounter}";
            sqlTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            // Query popup
            var popup = new Popup();
            popup.Name = "AutoCompletePopup";
            popup.StaysOpen = false;
            popup.AllowsTransparency = false;
            popup.PlacementTarget = sqlTextBox;
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
            popupListBox.Name = "PopupListBox";   

            // Elements relations
            popupBorder.Child = popupListBox;
            popup.Child = popupBorder;

            // Setting fields
            var context = new QueryEditorContext();
            context.Tab = tab;
            context.AutocompleteListBox = popupListBox;
            context.AutocompletePopup = popup;
            context.SqlTextBox = sqlTextBox;

            // Properties for elements
            sqlTextBox.TextChanged += (s, args) =>
            {
                ShowAutoComplete(context);
            };

            sqlTextBox.PreviewKeyDown += (s, args) =>
            {
                SqlTextBox_PreviewKeyDown(context, args);
            };

            popupListBox.MouseDoubleClick += (s, args) =>
            {
                InsertSelectedAutocomplete_DoubleClick(context);
            };

            tab.Content = sqlTextBox;

            _queryEditors[tab] = context;

            QueryTabs.Items.Add(tab);
            QueryTabs.SelectedItem = tab;
        }

        private void Button_CloseQuery(object sender, RoutedEventArgs e)
        {
            if (QueryTabs.SelectedItem is not TabItem selectedTab)
                return;

            if (_queryEditors.TryGetValue(selectedTab, out var context))
            {
                context.AutocompletePopup.IsOpen = false;
                _queryEditors.Remove(selectedTab);                
            }

            QueryTabs.Items.Remove(selectedTab);
            _queryCounter--;
        }
    }
}
