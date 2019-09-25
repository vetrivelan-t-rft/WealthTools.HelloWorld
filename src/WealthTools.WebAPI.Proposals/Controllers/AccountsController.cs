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
        [UnProtectParams(new string[] { "planid" })]
        public IActionResult AccountsSummary(string houseHoldId, string planId)
        {
            return String.IsNullOrWhiteSpace(houseHoldId) || String.IsNullOrWhiteSpace(planId) 
                ? BadRequest()
                : (IActionResult)Ok(_accountsRepository.GetAccountSummary(houseHoldId, planId));
        }

      
        /// <summary>
        /// API to get the different account types
        /// </summary>
        /// <returns></returns>
        [HttpGet("AccountTypes")]
        [MapToApiVersion("1.0")]
        public IActionResult AccountTypes()
        {
            return Ok(_accountsRepository.GetAccountTypes());
        }

        /// <summary>
        /// API to create a new account. Creates an account, with the name, account number, tax treatment, AccountType id,Ownership, if it is security/Asset level account 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPost("Create")]
        [MapToApiVersion("1.0")]
        public IActionResult CreateAccount([FromBody]AccountBasicInfoRequest account)
        {
            account.AccountID = _accountsRepository.CreateAccountBasicInfo(account);
            return Ok(account);

        }

        /// <summary>
        /// APi to delete account
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{accountId}")]
        [MapToApiVersion("1.0")]
        // [UnProtectParams(new string[] {"planid"})]       
        public IActionResult DeleteAccount(string accountId)
        {
            return Ok(_accountsRepository.DeleteAccount(accountId));
        }

        /// <summary>
        /// API to update and account updates only  the basic information .
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPost("Update")]
        [MapToApiVersion("1.0")]
        public IActionResult UpdateAccount([FromBody]AccountBasicInfoRequest account)
        {
            if (account == null || String.IsNullOrWhiteSpace(account.AccountID)) return BadRequest();
            return Ok(_accountsRepository.UpdateAccountBasicsInfo(account));

        }

       
    }
}