using Microsoft.AspNetCore.Mvc;
using System;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.Proposals.Interfaces;

namespace WealthTools.WebAPI.Proposals.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v1/Proposals")]
    public class ProposalsController : Controller
    {
        IProposalsRepository _proposalsRepository;
        IServiceProvider _serviceProvider;
        public ProposalsController(IProposalsRepository usuageLogRepository, IServiceProvider serviceProvider)
        {
            _proposalsRepository = usuageLogRepository;
            _serviceProvider = serviceProvider;
        }
        ///<summary>API to get the recent proposal list</summary>
        [HttpGet()]
        [MapToApiVersion("1.0")]
        public IActionResult RecentProposals()
        {
            return Ok(_proposalsRepository.GetRecentProposals());
        }
        ///<summary>API to get the list of proposals and archived reports for an household in context</summary>
        ///<param name="houseHoldId">Id of the household</param>
        [HttpGet("{houseHoldId}")]
        [MapToApiVersion("1.0")]
        public IActionResult ProposalByHH(string houseHoldId)
        {
            return Ok(_proposalsRepository.GetProposalsByHH(houseHoldId));
        }

        ///<summary>API to delete an existing proposal</summary>
        ///<param name="id">Id of the proposal to be deleted</param>
        [HttpDelete, Route("{id:int}")]        
        [MapToApiVersion("1.0")]
        public IActionResult DeleteInvestmentPlan(int id)
        {
            return Ok(_proposalsRepository.Delete_Web_Investment_Plan(id));
        }

        ///<summary>API to create a new proposal</summary>
        ///<param name="houseHoldID">Id of the household for which propsal needs to be created</param>
        ///<param name="proposalName">Name of the new proposal </param>
        [HttpPost("{houseHoldID}/{proposalName}")]
        [MapToApiVersion("1.0")]
        public IActionResult Create(string houseHoldID, string proposalName)
        {
            return Ok(_proposalsRepository.CreateNewProposal(houseHoldID,proposalName));
        }

    }
}