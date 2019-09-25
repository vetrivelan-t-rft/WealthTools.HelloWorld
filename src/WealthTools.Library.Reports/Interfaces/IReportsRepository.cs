using System;
using System.Collections.Generic;
using WealthTools.Library.Reports.Models;

namespace WealthTools.Library.Reports.Interfaces
{
    public interface IReportsRepository
    {
                
        List<ReportInfo> GetReportList(int ProductId);

        List<TemplateInfo> GetTemplateList(int ProductModuleID);

        List<TemplateReport> GetTemplateReportList(int ProductModuleID);


    }
}
