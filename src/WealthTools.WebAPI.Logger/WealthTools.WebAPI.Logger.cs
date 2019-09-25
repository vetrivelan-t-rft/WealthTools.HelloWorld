using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using WealthTools.Common.DatabaseConnection;
using WealthTools.Common.DatabaseWrapper;
using WealthTools.Common.DatabaseWrapper.Interfaces;
using WealthTools.Common.Logger;
using WealthTools.Common.Logger.Models;
using WealthTools.Common.Models;
using WealthTools.Common.Models.Interfaces;

namespace WealthTools.WebAPI.Logger
{
    public class UsageLogRepository : IUsageLogRepository
    {
        IDatabaseWrapper _dbWrapper;
        IContext _context;
        

        public UsageLogRepository()
        {
        }

        public UsageLogRepository(IContext context, IDatabaseWrapper dbWrapper)
        {
            _dbWrapper = dbWrapper;
            _context = context;
        }

        public List<UsageLogInfo> GetLastNLogs(int lastNLogs)
        {
            List<UsageLogInfo> usageLogList = new List<UsageLogInfo>();

            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd => cmd.CommandText = "SELECT logid, createddate, component,value from NXGEN_USAGELOG ", "usagelog");
            string logId = "";
            foreach (var row in records)
            {
                UsageLogInfo usageLogInfo = new UsageLogInfo();
                usageLogInfo.logid = row["logid"].ToString();
               // usageLogInfo.datetime = Convert.ToDateTime(row["createddate"].ToString());
                usageLogInfo.component = row["component"].ToString();
                usageLogInfo.usageData = row["value"].ToString();
                usageLogList.Add(usageLogInfo);
            }
            return usageLogList;
        }

        public string LogUsage(string usagedata)
        {
            return null;
        }

        List<UsageLogInfo> IUsageLogRepository.GetLastNLogs(int lastNLogs)
        {
            throw new NotImplementedException();
        }
    }
}
