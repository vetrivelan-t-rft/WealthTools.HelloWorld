using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using WealthTools.Common.DatabaseConnection;
using WealthTools.Common.DatabaseWrapper;
using WealthTools.Common.DatabaseWrapper.Interfaces;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.Reports.Interfaces;
using WealthTools.Library.Reports.Models;

namespace WealthTools.Library.Reports.Repositories
{
    public class ReportsRepository : IReportsRepository
    {
        IDatabaseWrapper _dbWrapper;
        IContext _context;
        IConnectionManager _connectionManager;        

        public ReportsRepository(IDatabaseWrapper dbWrapper, IContext context)
        {
            _dbWrapper = dbWrapper;
            _context = context;        
        }

       
       

        public List<ReportInfo> GetReportList(int ProductId)
        {
            List<ReportInfo> reportInfos = new List<ReportInfo>();
            reportInfos = FilterReports(GetInstiutionReportList(ProductId));
            
            List<int> PortFolioAnalyticsReport = new List<int>();            
            PortFolioAnalyticsReport.Add(PortfolioAnalyticsConstants.REPORT_ASSET_CLASS_GROWTH);
            PortFolioAnalyticsReport.Add(PortfolioAnalyticsConstants.REPORT_CALENDAR_YEAR_RETURNS);
            PortFolioAnalyticsReport.Add(PortfolioAnalyticsConstants.REPORT_RISK_VS_REWARD);
            PortFolioAnalyticsReport.Add(PortfolioAnalyticsConstants.REPORT_PORTFOLIO_HOLDINGS);
            PortFolioAnalyticsReport.Add(PortfolioAnalyticsConstants.REPORT_LIPPER_AWARDS);
            PortFolioAnalyticsReport.Add(PortfolioAnalyticsConstants.REPORT_HOLDING_PERIODS_VOLATALITY);
            PortFolioAnalyticsReport.Add(PortfolioAnalyticsConstants.REPORT_STOCK_CONVERGENCE);
            PortFolioAnalyticsReport.Add(PortfolioAnalyticsConstants.REPORT_PORTFOLIO_SNAPSHOT);

            reportInfos.Where(repo => PortFolioAnalyticsReport.Contains(repo.ReportId)).ToList().ForEach(rep => rep.TypeName = "Portfolio Analysis");

            return reportInfos;
        }

        private List<ReportInfo> GetInstiutionReportList(int ProductId)
        {
            List<ReportInfo> reportInfos = new List<ReportInfo>();
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SqlConstants.READ_REPORT_LIST;
                DatabaseWrapperHelper.AddInStringParameter(cmd, "INSTITUTION_ID", _context.Identity.InstitutionId);
                DatabaseWrapperHelper.AddInIntParameter(cmd, "PRODUCT_ID", ProductId.ToString());
            }, _context.Identity.InstitutionId);

            foreach (var row in records)
            {
                ReportInfo reportInfo = new ReportInfo();
                reportInfo.UiSeq = row["INST_REPORT_SEQ"] is DBNull ? 0 : Convert.ToInt32(row["INST_REPORT_SEQ"]);
                reportInfo.PrintSeq = row["INST_PRINT_SEQ"] is DBNull ? 0 : Convert.ToInt32(row["INST_PRINT_SEQ"]);
                reportInfo.UsageCode = row["INST_USAGE_CODE"].ToString();
                reportInfo.TypeId = row["REPORT_TYPE_ID"] is DBNull ? 0 : Convert.ToInt32(row["REPORT_TYPE_ID"]);
                reportInfo.TypeName = row["REPORT_TYPE_NAME"].ToString();
                reportInfo.ReportId = row["REPORT_ID"] is DBNull ? 0 : Convert.ToInt32(row["REPORT_ID"]);
                reportInfo.ReportName = row["REPORT_NAME"].ToString();                
                reportInfo.WorkflowTypeId = row["WORKFLOW_TYPE_ID"] is DBNull ? 0 : Convert.ToInt32(row["WORKFLOW_TYPE_ID"]);
                reportInfos.Add(reportInfo);
            }

            return reportInfos;
        }


        private List<ReportInfo> FilterReports(List<ReportInfo> reportInfos)
        {
               return reportInfos.Where(repo => repo.WorkflowTypeId ==14 && repo.UsageCode != "N" && repo.ReportId != 80).OrderBy(repo=>repo.UiSeq).ToList();
          
        }




        public List<TemplateInfo> GetTemplateList(int ProductModuleID)
        {
            throw new NotImplementedException();
        }

        public List<TemplateReport> GetTemplateReportList(int ProductModuleID)
        {
            List<TemplateReport> TemplateReports = new List<TemplateReport>();
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SqlConstants.READ_REPORT_TEMPLATE_LIST;
                DatabaseWrapperHelper.AddInStringParameter(cmd, "INSTITUTION_ID", _context.Identity.InstitutionId);
                DatabaseWrapperHelper.AddInIntParameter(cmd, "PRODUCTMODULE_ID", ProductModuleID.ToString());
            }, _context.Identity.InstitutionId);

            foreach (var row in records)
            {
                
                TemplateReport TemplateReport = new TemplateReport();

                //SELECT WRT.REPORT_TEMPLATE_ID,WRTR.REPORT_ID ,WRTR.REPORT_TYPE_ID, WRTR.INCLUDE_YN,WRTR.DISPLAY_OPTION




                TemplateReport.ReportId = row["REPORT_ID"] is DBNull ? 0 : Convert.ToInt32(row["REPORT_ID"]);
                TemplateReport.TemplateId = row["REPORT_TEMPLATE_ID"] is DBNull ? 0 : Convert.ToInt32(row["REPORT_TEMPLATE_ID"]);
                TemplateReport.TypeId = row["REPORT_TYPE_ID"] is DBNull ? 0 : Convert.ToInt32(row["REPORT_TYPE_ID"]);
                TemplateReport.IncludedYN = row["INCLUDE_YN"].ToString();
                TemplateReport.DisplayOption = row["DISPLAY_OPTION"].ToString();
                TemplateReports.Add(TemplateReport);
            }

            return TemplateReports;
        }
    }
}
