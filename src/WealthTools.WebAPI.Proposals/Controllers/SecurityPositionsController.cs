using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WealthTools.Common.Utils;
using WealthTools.Library.Accounts.Interfaces;
using WealthTools.Library.Accounts.Models;

namespace WealthTools.WebAPI.Proposals.Controllers
{
    /// <summary>
    /// Controller for AssetLevel Account Holdings(Psitions) svc
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v1/SecurityAcctPositions")]    
    public class SecurityPositionsController : Controller
    {
        IAccountsRepository _accountsRepository;
        IServiceProvider _serviceProvider;
        public SecurityPositionsController(IAccountsRepository usuageLogRepository, IServiceProvider serviceProvider)
        {
            _accountsRepository = usuageLogRepository;
            _serviceProvider = serviceProvider;
        }
       
        /// <summary>
        /// API to save the exclude and locked positions for a proposal
        /// </summary>        
        [HttpPost("LockExclude")]
        [MapToApiVersion("1.0")]
        public IActionResult SavePositions([FromBody] LockExcludeRequest lockExcludeRequest)
        {
            return lockExcludeRequest == null || lockExcludeRequest.accountPosition == null || string.IsNullOrWhiteSpace(lockExcludeRequest.planId)
                ? BadRequest()
                : (IActionResult)Ok(_accountsRepository.LockExcludePositions(lockExcludeRequest.planId, lockExcludeRequest.accountPosition));
        }


        /// <summary>
        /// API to save the exclude and locked positions for a proposal
        /// </summary>        
        [HttpPost("Save")]
        [MapToApiVersion("1.0")]
        public IActionResult SaveSeurityAccountPositions([FromBody] SaveSecurityPositionsRequest accountPosition)
        {
            if (accountPosition == null || String.IsNullOrWhiteSpace(accountPosition.AccountID)) return BadRequest();

            return Ok(_accountsRepository.SaveSeurityAccountPositions(accountPosition));
        }

    }
}