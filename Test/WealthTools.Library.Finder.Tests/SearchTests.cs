using WealthTools.Common.UnitTests.Utils;
using System;
using Xunit;
using WealthTools.Library.Search.Repositories;
using Microsoft.Extensions.DependencyInjection;
using WealthTools.Library.Contacts.Repositories;
using WealthTools.Library.Contacts.Interfaces;
using WealthTools.Library.Proposals.Repositories;
using WealthTools.Library.Proposals.Interfaces;
using WealthTools.Library.Search.Models;
using System.Collections.Generic;
using WealthTools.Library.Search.Interfaces;

namespace WealthTools.Library.Finder.Tests
{
    public class SearchTests : UnitTestBase
    {
        protected ISearchRepository _finderRepository;
        public  SearchTests()
        {
            _serviceCollection.AddScoped<ISearchRepository, SearchRepository>();
            _serviceCollection.AddScoped<IContactsRepository, ContactsRepository>();
            _serviceCollection.AddScoped<IProposalsRepository, ProposalsRepository>();
            // Build the service
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            _finderRepository = (ISearchRepository)_serviceProvider.GetService(typeof(ISearchRepository));

            // Modify the mock object in case needed like  _mockContext.Object.Identity.UserName = "PRatip";
            _mockContext.Object.Identity.BrokerId = "13240178";
            _mockContext.Object.Identity.InstitutionId = "6083";

        }
        [Theory]
        [MemberData(nameof(TestObjectGenerator.GetDemographicTestData), MemberType = typeof(TestObjectGenerator))]
        public void SearchContactsByDemographics(AdvanceSearchParams param)
        {
            SearchResult res = _finderRepository.Search(param);
            Assert.True(res!= null);
        }


        [Theory]
        [InlineData("cafax,msft")]
        public void SearchSecurities(string param)
        {
            SearchResult res = _finderRepository.SearchSecurities(param);
            Assert.True(res != null);
        }
    }

    public class TestObjectGenerator
    {
        public static IEnumerable<object[]> GetDemographicTestData()
        {


            yield return new object[]
            {
            new AdvanceSearchParams {
                LastName = "Smith",
                Count = 5
                 }
            };

        }
    }
    }
