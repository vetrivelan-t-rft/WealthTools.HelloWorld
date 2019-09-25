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
            _serviceCollection.AddScoped<BrokerManager.Interfaces.IBrokerMgrRepository, BrokerManager.Repositories.BrokerMgrRepository>();

            // Build the service
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            _accountsRepository = (IAccountsRepository)_serviceProvider.GetService(typeof(IAccountsRepository));

            // Modify the mock object in case needed like  _mockContext.Object.Identity.UserName = "PRatip";
            _mockContext.Object.Identity.BrokerId = "13240178";
            _mockContext.Object.Identity.InstitutionId = "6083";
        }
		
		[Theory]
        [InlineData("7778122", "214472")]
        // --[InlineData("7840092", "198761")]
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
        public void LockExcludePositions(LockExcludeRequest holdingRequest)
        {
            bool res = _accountsRepository.LockExcludePositions(holdingRequest.planId, holdingRequest.accountPosition);
            Assert.True(res);
        }

        [Theory]
        [MemberData(nameof(TestObjectGenerator.GetNegTestData), MemberType = typeof(TestObjectGenerator))]
        public void SavePositionsNegTest(LockExcludeRequest holdingRequest)
        {
            bool res = _accountsRepository.LockExcludePositions(holdingRequest.planId, holdingRequest.accountPosition);
            Assert.True(res==false);
        }

        [Fact]
        public void GetAccountTypes()
        {
            List<AccountType> res = _accountsRepository.GetAccountTypes();
            Assert.True(res!= null && res.Count >0);
        }

        [Theory]
        [MemberData(nameof(TestObjectGenerator.CreateAccountBasicInfo), MemberType = typeof(TestObjectGenerator))]
        public void CreateAccountBasicInfoTest(AccountBasicInfoRequest account)
        {
            string acctid = _accountsRepository.CreateAccountBasicInfo( account);
            Assert.True(!string.IsNullOrEmpty(acctid));
        }
        [Fact]
        public void GetACAccountPosition()
        {
            AssetClassAccount ACacct = _accountsRepository.GetAllAssetPositions();
            Assert.True(ACacct !=null && ACacct.ACPositions.Count > 0);
        }
        [Theory]
        [MemberData(nameof(TestObjectGenerator.CreateAccountBasicInfo), MemberType = typeof(TestObjectGenerator))]

        public void SaveACAccountPosition(AccountBasicInfoRequest account)
        {
            string acctid = _accountsRepository.CreateAccountBasicInfo(account);
            AssetClassAccount ACacct = _accountsRepository.GetAllAssetPositions();
            SaveAssetClassPositionsRequest req = new SaveAssetClassPositionsRequest()
            {
                AccountId = acctid,
                Balance = 1000,
                ACPositions = new List<SaveAssetClassPosition>()
                {
                    new SaveAssetClassPosition(){ assetClass = new SaveAssetClass(){SecId= ACacct.ACPositions[0].assetClass.SecId }, Pct =50},
                    new SaveAssetClassPosition(){ assetClass = new SaveAssetClass(){SecId= ACacct.ACPositions[1].assetClass.SecId }, Pct =50}
                }
            };
            bool res = _accountsRepository.SaveAssetAccountPositions(req);
            Assert.True(res);
        }

        [Theory]
        [MemberData(nameof(TestObjectGenerator.UpdateAccountBasicInfo), MemberType = typeof(TestObjectGenerator))]
        public void UpdateAccountBasicInfoTest(AccountBasicInfoRequest account)
        {
            bool res = _accountsRepository.UpdateAccountBasicsInfo(account);
            Assert.True(res);
        }

        [Theory]
        [MemberData(nameof(TestObjectGenerator.CreateAccountBasicInfo), MemberType = typeof(TestObjectGenerator))]
        public void deleteAccount(AccountBasicInfoRequest account)
        {
            string acctid = _accountsRepository.CreateAccountBasicInfo(account);
            bool res = _accountsRepository.DeleteAccount(acctid);
            Assert.True(res);
        }

        [Theory]
        [MemberData(nameof(TestObjectGenerator.SaveSeurityAccountPositionsTestData), MemberType = typeof(TestObjectGenerator))]
        public void SaveSeurityAccountPositions(SaveSecurityPositionsRequest accountPosition)
        {           
            
            bool res = _accountsRepository.SaveSeurityAccountPositions( accountPosition);
            Assert.True(res == true);
        }
    }

    public class TestObjectGenerator 
    {
        public static IEnumerable<object[]> SaveSeurityAccountPositionsTestData()
        {

            yield return new object[]
            {
                 new SaveSecurityPositionsRequest()
                 {
                      AccountID = "W-12271420",
                        CashComponent = 1000,
                         Positions = new List<SaveSecurityPosition>
                        {
                            new SaveSecurityPosition { SecID = "T-S-1101061",  Marketalue_entry_mode=(MarketValueEntry)1,Qty="100", CurrentPrice=104.19,
                                PositionTerm = (PositionTerm)1},
                             new SaveSecurityPosition { SecID = "T-S-1101061", Marketalue_entry_mode=(MarketValueEntry)1,Qty="999999999999.99", CurrentPrice=104.19,
                                PositionTerm = (PositionTerm)1, }
                        }
                  }

            };
        }
            public static IEnumerable<object[]> GetTestData()
            {
                var accPos = new LockExclAcct();
                accPos.AccountID = "W-12270594";
                //accPos.AccountDetail = AccountDetail.SECURITY;
                accPos.Positions = new List<LockExclPosition>
                {
                    new LockExclPosition { SecID = "T-S-1101061", LockedYN = true, ExcludeYN = false },
                    new LockExclPosition { SecID = "T-S-1420369", LockedYN = false, ExcludeYN = false },
                    new LockExclPosition { SecID = "T-S-2720677", LockedYN = false, ExcludeYN = false },
                    new LockExclPosition { SecID = "T-S-3091445", LockedYN = false, ExcludeYN = false }
                };

                yield return new object[]
                {
                new LockExcludeRequest {
                    planId = "212503",
                    accountPosition = accPos }
                };
            }

        public static IEnumerable<object[]> GetNegTestData()
        {
            var accPos = new LockExclAcct();
            accPos.AccountID = "-1";
            accPos.Positions = new List<LockExclPosition>
            {
                new LockExclPosition { SecID = "-1", LockedYN = true, ExcludeYN = false },
                new LockExclPosition { SecID = "-1", LockedYN = false, ExcludeYN = false },
            };

            yield return new object[]
            {
            new LockExcludeRequest {
                planId = "-1",
                accountPosition = accPos }
            };
        }
        public static IEnumerable<object[]> CreateAccountBasicInfo()
        {          

            yield return new object[]
            {
                
            new AccountBasicInfoRequest {
                HouseholdId ="7840092",
                 AccountName="test Account1",
                 TaxTreatment=TaxTreatment.TAXABLE,
                 AccountTypeId =30,
                 OwnershipType=OwnershipType.INDIVIDUAL,
                 AccountDetail =AccountDetail.SECURITY,
                 AccountNumber = "123456",
                 PrimaryOwnerId = "30966089"
                }
            };
        }


        public static IEnumerable<object[]> UpdateAccountBasicInfo()
        {

            yield return new object[]
            {

            new AccountBasicInfoRequest {
                AccountID ="W-12270914",
                 AccountName="test Account1",
                 TaxTreatment=TaxTreatment.TAXABLE,
                 AccountTypeId =30,
                 OwnershipType=OwnershipType.INDIVIDUAL,
                 AccountDetail =AccountDetail.SECURITY,
                 AccountNumber = "123456",
                 PrimaryOwnerId = "30966089"
                }
            };
        }


    }
}
