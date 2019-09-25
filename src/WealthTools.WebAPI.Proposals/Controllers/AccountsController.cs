using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WealthTools.Library.Accounts.Interfaces;
using WealthTools.Library.Accounts.Models;

namespace WealthTools.WebAPI.Proposals.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v1/Accounts")]
    public class AccountsController : Controller
    {
        IAccountsRepository _accountsRepository;
        IServiceProvider _serviceProvider;
        public AccountsController(IAccountsRepository usuageLogRepository, IServiceProvider serviceProvider)
        {
            _accountsRepository = usuageLogRepository;
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// Gets the accounts information for a proposal. 
        /// provides the list of all accounts, positions,  and the accounts included in the proposal 
        /// </summary>
        /// <param name="houseHoldId">Id of the household </param>
        /// <param name="planId">Proposal id </param>        
        [HttpGet("{houseHoldId}/{planId}")]
        [MapToApiVersion("1.0")]
        public IActionResult AccountsSummary(string houseHoldId, string planId)
        {
            return Ok(_accountsRepository.GetAccountSummary(houseHoldId, planId));
        }

        /// <summary>
        /// API to save the exclude and locked positions for a proposal
        /// </summary>        
        [HttpPost("SavePositions")]
        [MapToApiVersion("1.0")]
        public IActionResult SavePositions([FromBody] HoldingRequest holdingRequest)
        {
            return holdingRequest == null || holdingRequest.accountPosition == null || string.IsNullOrWhiteSpace( holdingRequest.planId)
                ? BadRequest()
                : (IActionResult)Ok(_accountsRepository.SavePositions(holdingRequest.planId, holdingRequest.accountPosition));
        }

    }
}