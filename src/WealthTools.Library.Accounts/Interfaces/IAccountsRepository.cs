using System;
using System.Collections.Generic;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.Accounts.Models;

namespace WealthTools.Library.Accounts.Interfaces
{
    public interface IAccountsRepository
    {
        List<AccountPosition> GetAccountSummary(string householdID, string planId);
        List<AccountType> GetAccountTypes();
        string CreateAccountBasicInfo(AccountBasicInfoRequest account);
        bool UpdateAccountBasicsInfo(AccountBasicInfoRequest account);
        bool DeleteAccount(string accountId);

        AssetClassAccount GetAllAssetPositions();
       
        bool SaveAssetAccountPositions(SaveAssetClassPositionsRequest account);
       

        bool SaveSeurityAccountPositions(SaveSecurityPositionsRequest account);

        Boolean LockExcludePositions(string planId, LockExclAcct accountPosition);

    }
}
