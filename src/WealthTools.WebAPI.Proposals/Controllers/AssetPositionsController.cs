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
    [Route("api/v1/AssetAcctPositions")]    
    public class AssetPositionsController : Controller
    {
        IAccountsRepository _accountsRepository;
        IServiceProvider _serviceProvider;
        public AssetPositionsController(IAccountsRepository usuageLogRepository, IServiceProvider serviceProvider)
        {
            _accountsRepository = usuageLogRepository;
            _serviceProvider = serviceProvider;
        }
       

        /// <summary>
        /// Gets all the default asset positions. Required for Creating/Updating new Asset class Account
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [HttpGet("All")]
        [MapToApiVersion("1.0")]
        public IActionResult GetAllAssetPositions(string accountId)
        {
            return Ok(_accountsRepository.GetAllAssetPositions());

        }
      

        /// <summary>
        /// Save asset level account Positions
        /// </summary>
        /// <param name="asssetClassPositions"></param>
        /// <returns></returns>
        [HttpPost("Save")]
        [MapToApiVersion("1.0")]
        public IActionResult Save([FromBody]SaveAssetClassPositionsRequest asssetClassPositions)
        {
            return Ok(_accountsRepository.SaveAssetAccountPositions(asssetClassPositions));

        }     

    }
}