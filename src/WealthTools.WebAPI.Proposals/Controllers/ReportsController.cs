using Microsoft.AspNetCore.Mvc;
using WealthTools.Common.ReportEngine;
using WealthTools.Library.Reports.Interfaces;

namespace WealthTools.WebAPI.Proposals.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v1/Reports")]
    public class ReportsController : Controller
    {
        IReportManager _reportMgr;
        IReportsRepository _reportsRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportsRepository"></param>
        /// <param name="reportMgr"></param>
        public ReportsController(IReportsRepository reportsRepository, IReportManager reportMgr)
        {
            _reportMgr = reportMgr;
            _reportsRepository = reportsRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("CreateReport")]
        [MapToApiVersion("1.0")]
        public IActionResult CreateReport([FromBody]ReportRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }
            return Ok(_reportMgr.GenerateReport(request));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [MapToApiVersion("1.0")]
        public IActionResult GetReportList()
        {
            //TODO: Move to constancts 
            int proudctId = 4;
            return Ok(_reportsRepository.GetReportList(proudctId));
        }
    }
}
