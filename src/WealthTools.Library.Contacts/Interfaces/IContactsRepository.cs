using System.Collections.Generic;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.Contacts.Models;

namespace WealthTools.Library.Contacts.Interfaces
{
    public interface IContactsRepository
    {
        List<Household> SearchAllContacts(SearchParameters searchParameters);

        List<Contact> GetContactsForHousehold(string householdId);

        Household CreateProspectHousehold(List<Contact> contactsListColl);
        List<Team> GetHouseHoldTeams(string householdId);
        bool CreateContact(Contact newCt, long householdId);
        bool UpdateContact(Contact contact);

    }
}
