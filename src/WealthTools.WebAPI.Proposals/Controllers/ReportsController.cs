using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WealthTools.Common.ReportEngine;

namespace WealthTools.WebAPI.Proposals.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v1/Reports")]
    public class ReportsController : Controller
    {
        IReportManager _reportMgr;
        IServiceProvider _serviceProvider;
        public ReportsController(IReportManager reportMgr, IServiceProvider serviceProvider)
        {
            _reportMgr = reportMgr;
            _serviceProvider = serviceProvider;
        }

        [HttpPost("CreateReport")]
        [MapToApiVersion("1.0")]
        public IActionResult CreateReport([FromBody]ReportRequest request)
        {
            if(request == null)
            {
                return BadRequest();
            }
            return Ok(_reportMgr.GenerateReport(request));

            
        }
    }
}
