using System;
using System.Collections.Generic;
using System.Text;

namespace WealthTools.Library.Reports.Models
{
     public class ReportInfo
    {

        public ReportInfo()
        {

        }

        public int ReportId { get; set; }

        public string ReportName { get; set; }

        public int PrintSeq { get; set; }
        public int UiSeq { get; set; }

        public int TypeId { get; set; }

        public string TypeName { get; set; }

        public string UsageCode { get; set; }

        public int WorkflowTypeId { get; set; }

        public bool Enabled { get; set; }
        
    }
}
