using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Studio
{
    public class HistoryQueries
    {
        public int? RowId { get; set; }
        public string ExecutionTime { get; set; }
        public string QueryText { get; set; }
    }
}
