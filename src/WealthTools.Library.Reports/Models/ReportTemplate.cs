using System;
using System.Collections.Generic;
using System.Text;

namespace WealthTools.Library.Reports.Models
{
     

    public class TemplateReport
    {

        public int TemplateId { get; set; }

        public int ReportId { get; set; }

        public int TypeId { get; set; }

        public string IncludedYN { get; set; }

        public string DisplayOption { get; set; }
    }

    public class TemplateInfo
    {
        public int TemplateId { get; set; }

        public string Name { get; set; }

        public string ActiveYN { get; set; }

        public bool IsDefault { get; set; }

        public int TemplateOrder { get; set; }
        
    }

}
