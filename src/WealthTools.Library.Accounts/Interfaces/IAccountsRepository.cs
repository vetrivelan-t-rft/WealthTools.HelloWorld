using System;
using System.Collections.Generic;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.Accounts.Models;

namespace WealthTools.Library.Accounts.Interfaces
{
    public interface IAccountsRepository
    {
        List<AccountPosition> GetAccountSummary(string householdID, string planId);
        Boolean SavePositions(string planId, AccountPosition accountPosition);
    }
}
