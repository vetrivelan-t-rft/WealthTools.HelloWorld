using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WealthTools.Common.Logger;
using WealthTools.Common.Logger.Interfaces;
using WealthTools.Common.Logger.Models;
using WealthTools.Common.Models;
using WealthTools.Common.Models.Interfaces;

namespace WealthTools.WebAPI.Logger.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v1/Usage")]
    public class UsageController : Controller
    {
        IWMLogger _logger;
        IUsageLogRepository _usuageLogRepository;

        private IContext _context => (IContext)_serviceProvider.GetService(typeof(IContext));
        IServiceProvider _serviceProvider;

        public UsageController(IWMLogger logger, IServiceProvider serviceProvider, IUsageLogRepository usuageLogRepository)
        {
            _logger = logger;
            _usuageLogRepository = usuageLogRepository;
            _serviceProvider = serviceProvider;
        }
        
        [HttpPost]
        [MapToApiVersion("1.0")]
        public void LogData([FromBody] string usageData)
        {
            _logger.LogUsage(usageData);
        }

        [HttpGet("{lastNLogs}")]
        [MapToApiVersion("1.0")]
        public List<UsageLogInfo> GetLastNLogs(int lastNLogs)
        {
           return  _usuageLogRepository.GetLastNLogs(lastNLogs);
        }
    }
}