using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WealthTools.Common.Logger;
using WealthTools.Common.Logger.Models;

namespace WealthTools.WebAPI.Logger
{
    public interface IUsageLogRepository
    {
        string LogUsage(string usagedata);
        List<UsageLogInfo> GetLastNLogs(int lastNLogs);
    }
}
