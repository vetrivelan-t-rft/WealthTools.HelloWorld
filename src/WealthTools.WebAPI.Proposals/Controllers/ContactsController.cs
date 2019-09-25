using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.Contacts.Interfaces;
using WealthTools.Library.Contacts.Models;

namespace WealthTools.WebAPI.Proposals.Controllers
{
    /// <summary>
    /// Controller for Contacts svc
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v1/Contacts")]
    public class ContactsController : Controller
    {
        readonly IContactsRepository _contactsRepository;
        readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="contactsRepository"></param>
        /// <param name="serviceProvider"></param>
        public ContactsController(IContactsRepository contactsRepository, IServiceProvider serviceProvider)
        {
            _contactsRepository = contactsRepository;
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// API to search contacts by either DemoGraphic details, Account Number or Rep code.
        /// Returns both client as well as prospect household
        /// </summary>        
        [HttpPost("Search")]
        [MapToApiVersion("1.0")]
        public IActionResult SearchContacts([FromBody] SearchParameters SearchParameters)
        {
            return SearchParameters == null ? BadRequest() 
                : (IActionResult)Ok(_contactsRepository.SearchAllContacts(SearchParameters));
        }
        /// <summary>
        /// returns the contact information for a given householdid
        /// </summary>
        /// <param name="houseHoldID"></param>
        /// <returns></returns>
        [HttpGet("{houseHoldId}")]
        [MapToApiVersion("1.0")]
        public IActionResult GetHouseholdContacts(string houseHoldID)
        {
            bool isValidHH = long.TryParse(houseHoldID, out long householdID);
            if (!isValidHH) return BadRequest();

            SearchResult result = new SearchResult();
            result.HouseholdID = houseHoldID;
            result.Persons = _contactsRepository.GetContactsForHousehold(result.HouseholdID);
            return Ok(result);       
        }


        [HttpPost()]
        [MapToApiVersion("1.0")]
        public IActionResult CreateProspectHousehold([FromBody]List<Contact> contacts)
        {
             if (contacts == null || contacts.Count == 0 || 
                !contacts.Exists(c=>c.RelationShipType == Library.Contacts.RelationShipType.HEAD && c.LastName != null) )
                return BadRequest();
            SearchResult result = _contactsRepository.CreateProspectHousehold(contacts);
            return Ok(result);
        }


    }
}