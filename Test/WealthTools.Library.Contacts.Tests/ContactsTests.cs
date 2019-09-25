using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Collections.Generic;
using WealthTools.Common.UnitTests.Utils;
using WealthTools.Library.Contacts.Interfaces;
using WealthTools.Library.Contacts.Models;
using WealthTools.Library.Contacts.Repositories;
using Xunit;

namespace WealthTools.Library.Contacts.Tests
{
    public class ContactsTests : UnitTestBase
    {
        protected IContactsRepository _contactsRepository;
        public ContactsTests()
        {
            // Add the service you want to add like _serviceCollection.AddSingleton<IEncrypt, EncryptDecryptAES>();
            _serviceCollection.AddScoped<IContactsRepository, ContactsRepository>();

            // Build the service
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            _contactsRepository = (IContactsRepository)_serviceProvider.GetService(typeof(IContactsRepository));

            // Modify the mock object in case needed like  _mockContext.Object.Identity.UserName = "PRatip";
            _mockContext.Object.Identity.BrokerId = "13240178";
            _mockContext.Object.Identity.InstitutionId = "6083";
        }
        

        [Theory]
        [MemberData(nameof(TestObjectGenerator.GetDemographicTestData), MemberType = typeof(TestObjectGenerator))]
        public void SearchContactsByDemographics(SearchParameters param)
        {
            List<Household> res = _contactsRepository.SearchAllContacts(param);
            Assert.True(res.Count > 0 );
        }

        [Theory]
        [MemberData(nameof(TestObjectGenerator.GetAcctSearchTestData), MemberType = typeof(TestObjectGenerator))]
        public void SearchContactsByAcctNo(SearchParameters param)
        {
            List<Household> res = _contactsRepository.SearchAllContacts(param);
            Assert.True(res.Count > 0);
        }

        [Theory]
        [MemberData(nameof(TestObjectGenerator.GetRepCodeSearchTestData), MemberType = typeof(TestObjectGenerator))]
        public void SearchContactsByRepCode(SearchParameters param)
        {
            List<Household> res = _contactsRepository.SearchAllContacts(param);
            Assert.True(res.Count == 0);
        }

        [Theory]
        [InlineData("7840092")]
        public void GetContactsForHousehold(string householdID)
        {
            List<Contact> res = _contactsRepository.GetContactsForHousehold(householdID);
            Assert.True(res.Count > 0);
        }

        [Theory]
        [InlineData("0")]
        public void GetContactsForHouseholdID0(string householdID)
        {
            List<Contact> res = _contactsRepository.GetContactsForHousehold(householdID);
            Assert.True(res.Count == 0);
        }

        [Theory]
        [InlineData("abcd")]
        public void GetContactsForInvalidHouseholdId(string householdID)
        {
            List<Contact> res = _contactsRepository.GetContactsForHousehold(householdID);
            Assert.True(res.Count == 0);
        }

        [Theory]
        [MemberData(nameof(TestObjectGenerator.ContactsTestData), MemberType = typeof(TestObjectGenerator))]

        public void CreateProspectHousehold(List<Contact> contacts)
        {
            Household res = _contactsRepository.CreateProspectHousehold(contacts);
            Assert.True((int.TryParse(res.HouseholdID, out int hhid)) && hhid > 0 && res.Persons.Count > 0);
        }

        [Theory]
        [InlineData("7840092")]
        public void GetTeamsForHousehold(string householdID)
        {
            List<Team> res = _contactsRepository.GetHouseHoldTeams(householdID);
            Assert.True(res.Count > 0);
        }

    }

    public class TestObjectGenerator 
    {
        public static IEnumerable<object[]> GetDemographicTestData()
        {
         

            yield return new object[]
            {
            new SearchParameters {
                LastName = "Smith",
                Count = 50
                 }
            };

        }

        public static IEnumerable<object[]> GetAcctSearchTestData()
        {


            yield return new object[]
            {
            new SearchParameters {
                Searchby=SearchBy.ACCOUNTNUMBER,
                AccountNumber = "1"
                 }
            };
        }

        public static IEnumerable<object[]> GetRepCodeSearchTestData()
        {


            yield return new object[]
            {
            new SearchParameters {
                Searchby=SearchBy.REPCODE,
                AccountNumber = "1"
                 }
            };
        }

        public static IEnumerable<object[]> ContactsTestData()
        {
            List<Contact> Contacts = new List<Contact>();
            Contact contact = new Contact() { LastName = "test80808" };
            Contacts.Add(contact);
            Contact contact1 = new Contact() { LastName = "test222" };
            Contacts.Add(contact1);

            yield return new object[]
            {

            new List<Contact>()
            {
                new Contact(){ LastName = "test80808",RelationShipType=RelationShipType.HEAD },
                new Contact(){LastName = "test222",RelationShipType=RelationShipType.DEPENDENT }
            }
            };

        }



    }
}
