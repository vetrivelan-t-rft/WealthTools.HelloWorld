using System.Collections.Generic;
using WealthTools.Common.ReportEngine;

namespace WealthTools.Library.Proposals.Models
{
    public class ProposalsModel
    {
        public string ProposalId { get; set; }
        public string ProposalName { get; set; }
        public string PartyID { get; set; }
        public string PartyName { get; set; }
        public string PartyType { get; set; }
        public string LastModifiedDate { get; set; }
        public string IsEntitle { get; set; }
        public int RowNum { get; set; }
        public string PlanTypeId { get; set; }
        public string ProgramName { get; set; }
        public string ProcessStatus { get; set; }
        public string IsNoPartialEntitlement { get; set; }
        public string ModelMinorVersion { get; set; }

    }

    public class ArchivedReportInfo
    {
        public string Name { get; set; }
        public string ReportId { get; set; }
        public string PlanId { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
        public string EntitledYN { get; set; }
        public string IS_STREETSCAPE { get; set; }
    }

    public class ProposalByHH : ProposalsModel
    {
       public List<ArchivedReportInfo> ReportInfoList { get; set; } = new List<ArchivedReportInfo>();
    }
}