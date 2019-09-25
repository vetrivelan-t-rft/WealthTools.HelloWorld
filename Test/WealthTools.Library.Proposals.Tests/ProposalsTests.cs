using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using WealthTools.Common.UnitTests.Utils;
using WealthTools.Library.Proposals.Interfaces;
using WealthTools.Library.Proposals.Models;
using WealthTools.Library.Proposals.Repositories;
using Xunit;

namespace WealthTools.Library.Proposals.Tests
{
    public class ProposalsTests : UnitTestBase
    {
        protected IProposalsRepository _proposalsRepository;
        public ProposalsTests()
        {
            // Add the service you want to add like _serviceCollection.AddSingleton<IEncrypt, EncryptDecryptAES>();
            _serviceCollection.AddScoped<IProposalsRepository, ProposalsRepository>();

            // Build the service
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            _proposalsRepository = (IProposalsRepository)_serviceProvider.GetService(typeof(IProposalsRepository));

            // Modify the mock object in case needed like  _mockContext.Object.Identity.UserName = "PRatip";
            _mockContext.Object.Identity.BrokerId = "13240190"; // user_id= ilxemm.im50t170@thomson.com
            _mockContext.Object.Identity.InstitutionId = "6083";
        }

        [Fact]
        public void RecentProposalTest()
        {
            List<ProposalsModel> propList = _proposalsRepository.GetRecentProposals();
            Assert.True(propList.Count > 0);
        }

        [Theory]
        [InlineData("7791708")]
        public void GetProposalsByHHTest(string houseHoldId)
        {
            List<ProposalByHH> propList = _proposalsRepository.GetProposalsByHH(houseHoldId);
            Assert.True(propList.Count > 0);
        }

        [Theory]
        [InlineData("-1")]
        public void GetProposalsByHHNegTest(string houseHoldId)
        {
            List<ProposalByHH> propList = _proposalsRepository.GetProposalsByHH(houseHoldId);
            Assert.True(propList.Count == 0);
        }

        [Theory]
        [InlineData("7791708", "test")]
        public void CreatePropsalForHH(string houseHoldId, string name)
        {
            long proposalId = _proposalsRepository.CreateNewProposal(houseHoldId,name);
            Assert.True(proposalId > 0);
        }


        [Theory]
        [InlineData("7791708", "test")]
        public void DeletePropsal(string houseHoldId, string name)
        {
            long proposalId = _proposalsRepository.CreateNewProposal(houseHoldId, name);
            Assert.True(proposalId > 0);

            bool isDeleted =  _proposalsRepository.Delete_Web_Investment_Plan( int.Parse(proposalId.ToString()));

            Assert.True(isDeleted, "Proposal should be delteted ");

        }
        [Theory]
        [InlineData("abc", "test")]
        public void CreatePropsalForInvalidHH(string houseHoldId, string name)
        {
            long proposalId = _proposalsRepository.CreateNewProposal(houseHoldId, name);
            Assert.True(proposalId > 0);
        }
        [Theory]
        [InlineData(null, null)]
        public void CreatePropsalForNullHH(string houseHoldId, string name)
        {
            long proposalId = _proposalsRepository.CreateNewProposal(houseHoldId, name);
            Assert.True(proposalId > 0);
        }

        [Theory]
        [MemberData(nameof(TestObjectGenerator.GetDemographicTestData), MemberType = typeof(TestObjectGenerator))]
        public void SearchProposalsByDemographics(ProposalSearchParameters param)
        {
            List<ProposalsModel> res = _proposalsRepository.SearchProposals(param);
            Assert.True(res.Count > 0);
        }

        [Theory]
        [MemberData(nameof(TestObjectGenerator.GetAcctSearchTestData), MemberType = typeof(TestObjectGenerator))]
        public void SearchProposalsByAcctNo(ProposalSearchParameters param)
        {
            List<ProposalsModel> res = _proposalsRepository.SearchProposals(param);
            Assert.True(res.Count > 0);
        }

    }
    public class TestObjectGenerator
    {
        public static IEnumerable<object[]> GetDemographicTestData()
        {


            yield return new object[]
            {
            new ProposalSearchParameters {
                LastName = "smith"
                 }
            };

        }

        public static IEnumerable<object[]> GetAcctSearchTestData()
        {


            yield return new object[]
            {
            new ProposalSearchParameters {
                Searchby=SearchBy.ACCOUNTNUMBER,
                AccountNumber = "1"
                 }
            };
        }
    }
    }
