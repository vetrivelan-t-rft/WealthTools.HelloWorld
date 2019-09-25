using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Collections.Generic;
using WealthTools.Common.UnitTests.Utils;
using WealthTools.Library.Accounts.Interfaces;
using WealthTools.Library.Accounts.Models;
using WealthTools.Library.Accounts.Repositories;
using Xunit;

namespace WealthTools.Library.Accounts.Tests
{
    public class AccountsTests : UnitTestBase
    {
        protected IAccountsRepository _accountsRepository;
        public AccountsTests()
        {
            // Add the service you want to add like _serviceCollection.AddSingleton<IEncrypt, EncryptDecryptAES>();
            _serviceCollection.AddScoped<IAccountsRepository, AccountsRepository>();

            // Build the service
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            _accountsRepository = (IAccountsRepository)_serviceProvider.GetService(typeof(IAccountsRepository));

            // Modify the mock object in case needed like  _mockContext.Object.Identity.UserName = "PRatip";
            _mockContext.Object.Identity.BrokerId = "13240178";
            _mockContext.Object.Identity.InstitutionId = "6083";
        }
		
		[Theory]
        [InlineData("7840092", "198761")]
        public void GetAccountSummaryTest(string householdID, string planId)
        {
            List<AccountPosition> res = _accountsRepository.GetAccountSummary(householdID, planId);
            Assert.True(res.Count > 0);
        }

        [Theory]
        [InlineData("-1", "-1")]
        public void GetAccountSummaryNegTest(string householdID, string planId)
        {
            List<AccountPosition> res = _accountsRepository.GetAccountSummary(householdID, planId);
            Assert.True(res.Count == 0);
        }

        [Theory]
        [MemberData(nameof(TestObjectGenerator.GetTestData), MemberType = typeof(TestObjectGenerator))]
        public void SavePositionsTest(HoldingRequest holdingRequest)
        {
            bool res = _accountsRepository.SavePositions(holdingRequest.planId, holdingRequest.accountPosition);
            Assert.True(res);
        }

        [Theory]
        [MemberData(nameof(TestObjectGenerator.GetNegTestData), MemberType = typeof(TestObjectGenerator))]
        public void SavePositionsNegTest(HoldingRequest holdingRequest)
        {
            bool res = _accountsRepository.SavePositions(holdingRequest.planId, holdingRequest.accountPosition);
            Assert.True(res==false);
        }

    }

    public class TestObjectGenerator 
    {
        public static IEnumerable<object[]> GetTestData()
        {
            var accPos = new AccountPosition();
            accPos.AccountID = "W-12270594";
            accPos.Account_detail = "SECURITY";
            accPos.Positions = new List<OpenPosition>
            {
                new OpenPosition { SecID = "T-S-1101061", Locked_yn = true, Exclude_yn = false },
                new OpenPosition { SecID = "T-S-1420369", Locked_yn = false, Exclude_yn = false },
                new OpenPosition { SecID = "T-S-2720677", Locked_yn = false, Exclude_yn = false },
                new OpenPosition { SecID = "T-S-3091445", Locked_yn = false, Exclude_yn = false }
            };

            yield return new object[]
            {
            new HoldingRequest {
                planId = "212503",
                accountPosition = accPos }
            };
        }

        public static IEnumerable<object[]> GetNegTestData()
        {
            var accPos = new AccountPosition();
            accPos.AccountID = "-1";
            accPos.Account_detail = "AAA";
            accPos.Positions = new List<OpenPosition>
            {
                new OpenPosition { SecID = "-1", Locked_yn = true, Exclude_yn = false },
                new OpenPosition { SecID = "-1", Locked_yn = false, Exclude_yn = false },
            };

            yield return new object[]
            {
            new HoldingRequest {
                planId = "-1",
                accountPosition = accPos }
            };
        }

    }
}
