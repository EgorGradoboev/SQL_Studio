using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SQL_Studio
{
    public class RecentConnection
    {
        public string? ServerName { get; set; }
        public string? Port { get; set; }
        public string? Login { get; set; }          
        
        public string DisplayName => $"{Login}@{ServerName}:{Port}";
    }
}
