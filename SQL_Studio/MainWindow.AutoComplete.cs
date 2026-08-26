using SQL_Studio.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace SQL_Studio
{
    public partial class MainWindow
    {
        private async void QueryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.DataContext is QueryViewModel vm)
            {
                await vm.UpdateAutoComplete(textBox.CaretIndex);

                var popup = (Popup)textBox.FindName("AutoCompletePopup");
                var rect = textBox.GetRectFromCharacterIndex(textBox.CaretIndex);
                popup.HorizontalOffset = rect.X;
                popup.VerticalOffset = rect.Y + rect.Height;
            }
        }
        private async void QueryTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox)sender;
            var listBox = (ListBox)textBox.FindName("SuggestionsListBox");
            if (textBox.DataContext is not QueryViewModel vm || !vm.IsAutoCompleteOpen)
                return;

            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                InsertSelectedSuggestion(textBox, vm, listBox);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                vm.IsAutoCompleteOpen = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                if (listBox.SelectedIndex < listBox.Items.Count - 1)
                    listBox.SelectedIndex++;
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (listBox.SelectedIndex > 0)
                    listBox.SelectedIndex--;
                e.Handled = true;
            }
        }
        private void InsertSelectedSuggestion(TextBox textBox, QueryViewModel vm, ListBox listBox)
        {
            if (listBox.SelectedItem is not string selectedTable)
                return;

            string currentWord = vm.GetCurrentWord(vm.QueryText, textBox.CaretIndex);
            int startIndex = textBox.CaretIndex - currentWord.Length;

            vm.IsAutocompleteInsert = true;

            textBox.SelectionStart = startIndex;
            textBox.SelectionLength = currentWord.Length;
            textBox.SelectedText = selectedTable;
            textBox.CaretIndex = startIndex + selectedTable.Length;

            vm.IsAutocompleteInsert = false;
            vm.IsAutoCompleteOpen = false;
        }
    }
}
