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
            return SearchParameters == null || (SearchParameters.Searchby == Library.Contacts.SearchBy.DEMOGRAPHICS && String.IsNullOrWhiteSpace(SearchParameters.LastName) )? BadRequest() 
                : (IActionResult)Ok(_contactsRepository.SearchAllContacts(SearchParameters));
        }
        /// <summary>
        /// returns the contact information for a given householdid
        /// </summary>
        /// <param name="houseHoldID">Id of the household</param>
        /// <returns></returns>
        [HttpGet("{houseHoldID}")]
        [MapToApiVersion("1.0")]
        public IActionResult GetHouseholdContacts(string houseHoldID)
        {
            bool isValidHH = long.TryParse(houseHoldID, out long householdID);
            if (!isValidHH) return BadRequest();

            Household result = new Household();
            result.HouseholdID = houseHoldID;
            result.Persons = _contactsRepository.GetContactsForHousehold(result.HouseholdID);
            return Ok(result);       
        }

        /// <summary>
        /// creates a prospect household
        /// </summary>
        /// <param name="contacts">List of contacts</param>
        /// <returns></returns>
        [HttpPost()]
        [MapToApiVersion("1.0")]
        public IActionResult CreateProspectHousehold([FromBody]ContactList contacts)
        {
             if (contacts == null || contacts.list == null || contacts.list.Count == 0 || 
                !contacts.list.Exists(c=>c.RelationShipType == Library.Contacts.RelationShipType.HEAD && !String.IsNullOrWhiteSpace(c.LastName)) )
                return BadRequest();
            Household result = _contactsRepository.CreateProspectHousehold(contacts.list);
            return Ok(result);
        }

        /// <summary>
        /// creates a contact
        /// </summary>
        /// <param name="contact">contact information</param>
        /// <returns></returns>
        [HttpPost("Create")]
        [MapToApiVersion("1.0")]
        public IActionResult CreateContact([FromBody]Contact contact)
        {
             if (contact == null || contact.RelationShipType == Library.Contacts.RelationShipType.HEAD || !long.TryParse(contact.HouseholdId, out long value))
                return BadRequest();
            bool result = _contactsRepository.CreateContact(contact, Convert.ToInt64(contact.HouseholdId));
            return Ok(result);
        }

        /// <summary>
        /// updates a contact
        /// </summary>
        /// <param name="contact">contact information</param>
        /// <returns></returns>
        [HttpPost("Update")]
        [MapToApiVersion("1.0")]
        public IActionResult UpdateContact([FromBody]Contact contact)
        {
            if (contact == null || contact.RelationShipType == Library.Contacts.RelationShipType.HEAD || !long.TryParse(contact.InvestorId, out long investorId) || string.IsNullOrEmpty(contact.LastName))
                return BadRequest();
            bool result = _contactsRepository.UpdateContact(contact);
            return Ok(result);
        }

    }
}