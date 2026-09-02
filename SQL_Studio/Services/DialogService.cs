using SQL_Studio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SQL_Studio.Services
{
    public class DialogService : IDialogService
    {
        public void ShowError(string message) => MessageBox.Show(message);
    }
}
