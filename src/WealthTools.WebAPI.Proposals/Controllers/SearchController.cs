using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WealthTools.Common.Models.Interfaces;

using WealthTools.Library.Search.Models;
using WealthTools.Library.Search.Interfaces;

namespace WealthTools.WebAPI.Proposals.Controllers
{
    /// <summary>
    /// Controller for Contacts svc
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v1/Search")]
    public class SearchController : Controller
    {
        readonly ISearchRepository _finderRepository;
        readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="finderRepository"></param>
        /// <param name="serviceProvider"></param>
        public SearchController(ISearchRepository finderRepository, IServiceProvider serviceProvider)
        {
            _finderRepository = finderRepository;
            _serviceProvider = serviceProvider;
        }
       /// <summary>
       /// API for advance search, Searches for Proposals and household
       /// </summary>
       /// <param name="SearchParameters"></param>
       /// <returns></returns>
        [HttpPost("AdvanceSearch")]
        [MapToApiVersion("1.0")]
        public IActionResult AdvanceSearch([FromBody] AdvanceSearchParams SearchParameters)
        {
            return SearchParameters == null ? BadRequest()
                : (IActionResult)Ok(_finderRepository.Search(SearchParameters));
        }

        /// <summary>
        /// Security search. search either on Symbol or CUSIP
        /// </summary>
        /// <param name="SearchQuery"></param>
        /// <returns></returns>
        [HttpGet("{SearchQuery}")]
        [MapToApiVersion("1.0")]
        public IActionResult SearchSecurities(string SearchQuery)
        {
            return String.IsNullOrEmpty(SearchQuery) ? BadRequest()
                : (IActionResult)Ok(_finderRepository.SearchSecurities(SearchQuery));
        }


    }
}