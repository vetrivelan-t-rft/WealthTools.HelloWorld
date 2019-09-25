using System.Collections.Generic;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.Contacts.Models;

namespace WealthTools.Library.Contacts.Interfaces
{
    public interface IContactsRepository
    {
        List<SearchResult> SearchAllContacts(SearchParameters searchParameters);

        List<Contact> GetContactsForHousehold(string householdId);

        SearchResult CreateProspectHousehold(List<Contact> contactsListColl);
    }
}
