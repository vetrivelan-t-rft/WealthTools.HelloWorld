using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using WealthTools.Common.ReportEngine;

namespace WealthTools.Library.Reports
{
    public class ProposalsReportFactory : IReportFactory
    {
        IServiceProvider _serviceProvider;
        public ProposalsReportFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IReportProcessor GetReportProcessor(string reportId)
        {
            Type type = null;
            ReportIDsEnum a_inputID = (ReportIDsEnum)Enum.Parse(typeof(ReportIDsEnum), reportId);
            switch (a_inputID)
            {
                case ReportIDsEnum.COVER_PAGE:
                     type = Type.GetType("WealthTools.Library.Reports.CoverPage,WealthTools.Library.Reports");
                    break;

            }
            if (type == null)
                throw new Exception("Unknown type for controller: ");


            Object Obj = _serviceProvider.GetService(type);
            IReportProcessor result = Obj as IReportProcessor;
            return result;
        }

    }
}
