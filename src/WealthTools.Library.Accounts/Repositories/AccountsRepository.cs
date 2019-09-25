using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using WealthTools.Common.DatabaseConnection;
using WealthTools.Common.DatabaseWrapper;
using WealthTools.Common.DatabaseWrapper.Interfaces;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.Accounts.Handlers;
using WealthTools.Library.Accounts.Interfaces;
using WealthTools.Library.Accounts.Models;
using WealthTools.Library.BrokerManager.Interfaces;

namespace WealthTools.Library.Accounts.Repositories
{
    public class AccountsRepository : IAccountsRepository
    {
        IDatabaseWrapper _dbWrapper;
        IContext _context;
        IConnectionManager _connectionManager;
        IBrokerMgrRepository _brokerMgrRepository;

        public AccountsRepository(IDatabaseWrapper dbWrapper, IContext context, IBrokerMgrRepository brokerMgrRepository)
        {
            _dbWrapper = dbWrapper;
            _context = context;
            _brokerMgrRepository = brokerMgrRepository;
        }

        public List<AccountPosition> GetAccountSummary(string householdID, string planId)
        {
            List<AccountPosition> listOfAccounts = new List<AccountPosition>();
            int _householdID = -1;
            if (!String.IsNullOrWhiteSpace(householdID)  && !String.IsNullOrWhiteSpace(planId)) {
                if (!Int32.TryParse(householdID, out _householdID) || !Int32.TryParse(planId, out int _planID)) return listOfAccounts;
               // _householdID = Int32.Parse(householdID);
                List<AccountKeyInfo> accts = GetAccountsForHousehold(householdID);
                List<string> acctIds = new List<string>();
                if (accts != null && accts.Count > 0)
                {
                    for (int cnt = 0; cnt < accts.Count; cnt++)
                    {
                        acctIds.Add(((AccountKeyInfo)accts[cnt]).AccountID);
                    }
                    listOfAccounts = GetPositions(acctIds, false); // bIncludeClosedActs hard coded for now
                }

                //Get the list of accounts selected
                if (listOfAccounts.Count > 0)
                {
                    string initalPortfolioId = "-1";
                    if (Convert.ToInt32(planId) != 0)
                    {
                        CalculatePlanAccountMarketValue(ref listOfAccounts);
                        //Get initialPortfolio id for the plan_id
                        if (Convert.ToInt32(planId) > 0)
                        {
                            initalPortfolioId = GetInitalPortfolioId(planId);
                        }
                        FilterOutPostions(initalPortfolioId, ref listOfAccounts);
                    }
                    //RemovePositions(ref listOfAccounts);

                    //get asset classification details for each securities
                    getAssetClassification(ref listOfAccounts);
                }
                
            }
            return listOfAccounts;
        }

        public List<AccountKeyInfo> GetAccountsForHousehold(string householdID)
        {
            List<AccountKeyInfo> accountsResult = new List<AccountKeyInfo>();
            bool bEligibleForBank = GetIsEligibleForBankAccounts();
            string loggedInInstitution = GetLoggedInInstitution() == "2" ? "Y" : "N";
            string sql = "";
            sql = bEligibleForBank ? SqlConstants.READHOUSEHOLDACCOUNTS_SQL : SqlConstants.READHOUSEHOLDACCOUNTS_SKIPBANK_SQL;
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                DatabaseWrapperHelper.AddInIntParameter(cmd, "broker_id", _context.Identity.BrokerId);
                DatabaseWrapperHelper.AddInIntParameter(cmd, "household_id", householdID);
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":boInstitute", loggedInInstitution); //Y-BO,N-TA
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":bIncludeClosedActs", "N");
            }, _context.Identity.InstitutionId);

            foreach (var row in records)
            {
                AccountKeyInfo account = new AccountKeyInfo();
                account.AccountID = row["ACCOUNT_ID"].ToString();
                account.AccountName = row["ACCOUNT_NAME"].ToString();
                account.AccountNumber = row["ACCOUNT_NUM"].ToString();
                account.Attribute2 = row["ATTR2"].ToString();
                account.DisplayTypeId = row["DISPLAY_TYPE_ID"].ToString();
                account.InternalYN = row["INTERNAL_YN"].ToString();
                account.ProgramID = row["PROGRAM_ID"].ToString();
                //account.TestAccount = Convert.ToBoolean(row["TEST_YN"]);
                account.ErrorMessage = "";
                accountsResult.Add(account);
            }
            return accountsResult;
        }

        private string GetLoggedInInstitution()
        {
            string instCfg = null;
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SqlConstants.LOGGED_INSTITUTION_ID_SQL;
                DatabaseWrapperHelper.AddInIntParameter(cmd, "INSTITUTION_ID", _context.Identity.InstitutionId);
            }, _context.Identity.InstitutionId);

            foreach (var row in records)
            {
                instCfg = row["value"].ToString();
            }
            return instCfg;
        }

        //TODO: there would be new way to fetch the preferences. until then, hardcoding return value for now. (AS PER DISCUSSION WITH PRATIP)
        public bool GetIsEligibleForBankAccounts()
        {
            //String commaSeparatedTeams = PreferenceContainer.GetPreferences(context.InstitutionId, context.BrokerId)[(int)ePreferenceId.BANK_ACCOUNT_ENTITLED_TEAMS];
            bool result = true;

            //if (!String.IsNullOrEmpty(commaSeparatedTeams) && !String.Equals(commaSeparatedTeams, "N/A"))
            //    result = AccountPositionsDataGateway.IsBrokerInTeams(context, commaSeparatedTeams);

            return result;
        }

        public List<AccountPosition> GetPositions(List<string> acctIds, bool bIncludeClosedActs)
        {
            List<AccountPosition> acctDetail = new List<AccountPosition>();
            string makeCSV = GetCommaSeperatedStrings(acctIds);
            string sql = "";
            sql = SqlConstants.READ_INTERNAL_AND_EXTERNAL_ACCOUNT_DETAILS_SQl;
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                DatabaseWrapperHelper.AddInStringParameter(cmd, "account_ids", makeCSV);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "bIncludeClosedActs", "N");
            }, _context.Identity.InstitutionId);

            foreach (var row in records)
            {
                AccountPosition acct = new AccountPosition();
                acct.AccountID = row["ACCOUNT_ID"].ToString();
                acct.AccountName = row["ACCOUNT_NAME"].ToString();
                acct.IrsName = row["IRS_NAME"].ToString();
                acct.OwnerName = row["OWNER_NAME"].ToString();
                acct.AccountNumber = row["ACCOUNT_NUM"].ToString();
                acct.ATTR2 = row["ATTR2"].ToString();
                acct.ProgramId = row["PROGRAM_ID"].ToString();
                acct.CashComponent = row["CASH_COMPONENT"] is DBNull ? 0 : Convert.ToDouble(row["CASH_COMPONENT"]);
                acct.CurrencyCode = row["CURRENCY_CODE"].ToString();
                acct.Rate = row["RATE"] is DBNull ? 0.0 : Convert.ToDouble(row["RATE"]);
                acct.LongBalance = row["LONG_MARKET_VALUE"] is DBNull ? 0 : Convert.ToDouble(row["LONG_MARKET_VALUE"]);
                acct.ShortBalance = row["SHORT_MARKET_VALUE"] is DBNull ? 0 : Convert.ToDouble(row["SHORT_MARKET_VALUE"]);
                acct.MarketValue = row["MARKET_VALUE"] is DBNull ? 0 : Convert.ToDouble(row["MARKET_VALUE"]);
                acct.InternalValueDBField = row["INTERNAL_MANAGED_YN"].ToString();
                acct.ManualDBField = row["MANUAL_FLAG"].ToString() != "N";
                acct.TaxTreatment = row["TAX_STATUS_ID"] is DBNull ? TaxTreatment.DEFERRED: (TaxTreatment)Convert.ToInt64(row["TAX_STATUS_ID"]);
                acct.NatureOfAcct = row["NATURE_OF_ACCOUNT"].ToString();
                acct.OwnershipType = row["FILING_STATUS_ID"] is DBNull ? OwnershipType.JOINT : (OwnershipType)(Convert.ToInt32(row["FILING_STATUS_ID"]));
                acct.AccountTypeId = row["ACCOUNT_TYPE_ID"] is DBNull ? 0 : Convert.ToInt64(row["ACCOUNT_TYPE_ID"]);
                acct.PrimaryOwnerId = row["PRIMARY_OWNER_ID"].ToString();
                acct.AcctOwnerId = row["ACCT_OWNER_ID"].ToString();
                acct.SecondaryOwnerId = row["SECONDARY_OWNER_ID"].ToString();
                acct.AccountDetail = row["ACCOUNT_DETAIL"].ToString() == "Y"? AccountDetail.SECURITY:AccountDetail.ASSET_CLASS;
                acct.LastUpdatedDate = row["last_modified_date"].ToString();
                acct.PortfolioAcctType = row["PORTFOLIO_ACCT_TYPE"].ToString();
                acct.DatasourceID = row["DATASOURCE_ID"] is DBNull ? 0 : Convert.ToInt32(row["DATASOURCE_ID"]);

                acctDetail.Add(acct);
            }

            if (acctDetail != null)
            {
                List<string> activeActIds = new List<string>();
                foreach (AccountPosition acc in acctDetail)
                {
                    activeActIds.Add(acc.AccountID);
                }
                GetPositionsForInternalAndExternalAccounts(acctDetail, activeActIds);
            }

            return acctDetail;
        }

        public void GetPositionsForInternalAndExternalAccounts(List<AccountPosition> acctDetail, List<string> acctIds)
        {
            List<string> tblSecIds = new List<string>();
            string sql = SqlConstants.READ_INTERNAL_ANDE_XTERNAL_POSITIONS_SQl;
            sql = _dbWrapper.BuildSqlInClauseQuery(acctIds, ":ACCOUNT_IDS", sql);
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                _dbWrapper.BuildParamInClauseQuery(acctIds, "ACCOUNT_IDS", cmd);
            }, _context.Identity.InstitutionId);

            foreach (var row in records)
            {
                AccountPosition prevAct = null;
                OpenPosition position = new OpenPosition();
                position.AccountID = row["ACCOUNT_ID"].ToString();
                position.Commissions = row["COMMISSION"] is DBNull ? 0 : Convert.ToDouble(row["COMMISSION"]);
                position.EstimatedAnnualIncome = row["ESTIMATED_ANNUAL_INCOME"] is DBNull ? 0 : Convert.ToDouble(row["ESTIMATED_ANNUAL_INCOME"]);
                position.IsMarketValuePresetYN = row["MARKET_VALUE_PRESET"].ToString();
                position.MarketValue_entry_modeDB = row["MKT_VAL_ENTRY_MODE"].ToString();
                position.OpenDate = row["PURCHASE_DATE"].ToString();
                position.OpenPrice = row["PURCHASE_PRICE"] is DBNull ? 0 : Convert.ToDouble(row["PURCHASE_PRICE"]);
                position.PositionTermDB = row["POSITION_TERM"].ToString();
                position.PosId = row["POSITION_ID"] is DBNull ? 0 : Convert.ToInt64(row["POSITION_ID"]);
                position.Qty = row["QUANTITY"] is DBNull ? 0 : Convert.ToDouble(row["QUANTITY"]);
                position.SecID = row["SEC_ID"].ToString();

                if (position != null)
                {
                    if (prevAct == null || prevAct.AccountID != position.AccountID)
                    {
                        foreach (AccountPosition acc in acctDetail)
                        {
                            if (acc.AccountID == position.AccountID)
                                prevAct = acc;
                        }
                    }
                    if (prevAct != null) prevAct.Positions.Add(position);
                    tblSecIds.Add(position.SecID);
                }
            }

            if (tblSecIds.Count > 0)
            {
                string sqlChunks = SqlConstants.READ_SECURITYDETAILS_SQL;
                sqlChunks = _dbWrapper.BuildSqlInClauseQuery(tblSecIds, ":SEC_IDS", sqlChunks);
                IEnumerable<IDataRecord> recordsChunks = _dbWrapper.QueryDataRecord(cmd =>
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sqlChunks;
                    _dbWrapper.BuildParamInClauseQuery(tblSecIds, "SEC_IDS", cmd);
                }, _context.Identity.InstitutionId);
                foreach (var row in recordsChunks)
                {
                    foreach (AccountPosition account in acctDetail)
                    {
                        foreach (OpenPosition position in account.Positions)
                        {
                            string sec_id = row["SEC_ID"].ToString();
                            if (sec_id == position.SecID)
                            {
                                position.CogId = row["COG_ID"].ToString();
                                position.CurrencyCode = row["CURRENCY_CODE"].ToString();
                                position.CurrentFactor = row["CURRENT_FACTOR"] is DBNull ? 0.0 : Convert.ToDouble(row["current_factor"]);
                                position.CurrentPrice = row["CURRENT_PRICE"] is DBNull ? 0.0 : Convert.ToDouble(row["CURRENT_PRICE"]);
                                position.PricingDate = row["AS_OF_DATE"].ToString();
                                position.SecurityDetail.Cusip = row["CUSIP"].ToString();
                                position.SecurityDetail.SecSedol = row["SECURITY_SEDOL"].ToString();
                                position.SecurityDetail.SecName = row["NAME"].ToString();
                                position.SecurityDetail.SecType = row["SEC_TYPE"].ToString();
                                position.SecurityDetail.SubType = row["SUB_TYPE"].ToString();
                                position.SecurityDetail.SecSymbol = row["TICKER"].ToString();
                                position.SecurityDetail.PriceFactor = row["PRICE_FACTOR"] is DBNull ? 0 : Convert.ToDouble(row["PRICE_FACTOR"]);
                                position.SecurityDetail.Price_ADJ_factor = row["PRICE_ADJ_FACTOR"] is DBNull ? 0 : Convert.ToDouble(row["PRICE_ADJ_FACTOR"]);
                                position.SecurityDetail.CurrencyCode = row["CURRENCY_CODE"].ToString();
                                position.SecurityDetail.CurrencyConvFactor = row["RATE"] is DBNull ? 0 : Convert.ToDouble(row["RATE"]);
                                position.SecurityDetail.SecStatus = row["STATUS"].ToString();
                                position.SecurityDetail.FileExists = row["FILEEXISTS"] is DBNull ? false : Convert.ToBoolean(row["FILEEXISTS"]);
                                if (position.CurrentPrice < PriceHelper.ZeroPrice)
                                {
                                    double price = PriceHelper.DefaultPrice;
                                    if(position.SecurityDetail.SecType != null)
                                    if (position.SecurityDetail.SecType.CompareTo("FI") == 0)
                                        price *= 100.0;
                                    position.CurrentPriceDB = price;
                                }
                                //For AC level securities, the as of date is null. Hence, update the PricingDate
                                //with the last modified date of the account.
                                if (position.SecurityDetail.SecType == "AC")
                                {
                                    if (position.CurrentPrice < PriceHelper.ZeroPrice)
                                        position.CurrentPriceDB = PriceHelper.DefaultPrice;
                                    position.PricingDate = account.LastModifiedDate;
                                }
                                // set HasFactSheet flag
                                // position.SecurityDetail.HasFactSheet = GetFactSheetInfo(context, position);
                            }
                        }
                    }
                }

            }

            //Now remove the inactive securities or security does not exist in security view at all
            foreach (AccountPosition account in acctDetail)
            {
                List<OpenPosition> removedItems = new List<OpenPosition>();
                foreach (OpenPosition position in account.Positions)
                {
                    if (position != null && (position.SecurityDetail.SecStatus == "" || position.SecurityDetail.SecStatus == "IN"))
                        removedItems.Add(position);
                }

                foreach (OpenPosition item in removedItems)
                    account.Positions.Remove(item);
            }

            //Now calculate the balanaces
            for (int index = 0; index < acctDetail.Count; index++)
            {
                AccountPosition account = (AccountPosition)acctDetail[index];

                //Just make sure that the balance is properly calculated for external accounts
                //For internal accounts this value is obtained from bo_account_balance table
                if (account != null) //&& (account.InternalValue == false || account.Manual == true))
                {
                    account.MarketValue = AccountBalanceTraits.CalculateBalance(account);
                    account.LongBalance = AccountBalanceTraits.CalculateLongBalance(account);
                    account.ShortBalance = AccountBalanceTraits.CalculateShortBalance(account);
                }
            }

            //For each of the accounts calculate account balances
            foreach (AccountPosition act in acctDetail)
            {
                if (act.InternalValue == false)
                {
                    act.AccountCashBalance = AccountBalanceTraits.CalculateBalance(act);
                    string lastUpdate = AccountDateTraits.CalculateLastUpdatedDate(act);
                    if (lastUpdate.Length != 0)
                        act.LastUpdatedDate = lastUpdate;
                }
            }
        }
        
        private void CalculatePlanAccountMarketValue(ref List<AccountPosition> listOfAccounts)
        {
            double accountLongBalance = 0;
            foreach (AccountPosition account in listOfAccounts)
            {
                int noOfSecurity = 0;
                foreach (PositionBase position in account.Positions)
                {
                    noOfSecurity += 1;
                    if ((position.PositionTerm == PositionTerm.LONG)
                            && (position.MarketValue > 0))
                    {
                        accountLongBalance += position.MarketValue;
                    }
                }
                account.SecCount = noOfSecurity;
                account.LongBalance = accountLongBalance;
                account.MarketValue = account.LongBalance
                                                        + ((account.CashComponent > 0) ? account.CashComponent : 0);

                accountLongBalance = 0;
            }
        }

        private void FilterOutPostions(string initalPortfolioId, ref List<AccountPosition> a_accountposition)
        {
            //Get locked and/or included securities from the web_poisiton_filter table
            PositionFilterCollection positionCollection = new PositionFilterCollection();
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SqlConstants.GET_POSITION_FILTER_SQL;
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":a_PORTFOLIO_ID", initalPortfolioId);
            }, _context.Identity.InstitutionId);
            foreach (var row in records)
            {
                PositionFilter positionFilter = new PositionFilter();
                positionFilter.AccountId = row["ACCOUNT_ID"].ToString();
                positionFilter.PortfolioId = row["PORTFOLIO_ID"].ToString();
                positionFilter.SecId = row["SEC_ID"].ToString();
                positionFilter.lockedYN = row["LOCKED_YN"].ToString().ToUpper() == "Y";
                positionFilter.ExcludeYN = row["EXCLUDE_YN"].ToString().ToUpper() == "Y";
                positionFilter.Pct = Int16.Parse(row["PCT"].ToString());
                positionCollection.Add(positionFilter);
            }

            AccountPosition _memento = new AccountPosition();
            if (initalPortfolioId != "-1" && positionCollection.Count > 0)
            {
                foreach (PositionFilter postion in positionCollection.List)
                {
                    if (postion.AccountId != null)
                    {
                        AccountPosition apIterator = (AccountPosition)a_accountposition.Find(
                                                     delegate (AccountPosition match){return ((AccountPosition)match).AccountID == postion.AccountId;});
                        if (apIterator != null)
                        {
                            string accountID = apIterator.AccountID;
                            if (postion.AccountId == accountID && postion.Pct == 100)
                                apIterator.IncludeAccountYN = "Y";
                            
                                foreach (PositionBase opm in apIterator.Positions)
                                {
                                    opm.SecurityDetail.SecName = (opm.SecurityDetail.SecName == string.Empty) ? "--" : opm.SecurityDetail.SecName;

                                    //if the market value for shot postion is not negative, make it negative
                                    if ((opm.PositionTerm == PositionTerm.SHORT) && (opm.MarketValue > 0))
                                        opm.MarketValue = (opm.MarketValue * -1.0);
                                    //if ((opm.PositionTerm == PositionBase.tPositionTerm.SHORT) && (opm.Qty > 0))
                                      //  opm.Qty = (opm.Qty * -1.0);
                                    if (positionCollection.Count > 0)
                                    {
                                        if (postion.SecId == opm.SecID && postion.AccountId == opm.AccountID)
                                        {
                                            opm.LockedYN = postion.lockedYN;
                                            opm.ExcludeYN = postion.ExcludeYN;

                                            if (postion.ExcludeYN == true)//subtract the market value
                                            {
                                                if (opm.PositionTerm.Equals(PositionTerm.LONG) && opm.MarketValue > 0)
                                                {
                                                    apIterator.LongBalance = apIterator.LongBalance - opm.MarketValue;
                                                    apIterator.MarketValue = apIterator.MarketValue - opm.MarketValue;
                                                }
                                            }
                                        }
                                    }

                                }
                            
                        }
                    }
                }
            }
        }

        public string GetCommaSeperatedStrings(List<string> IDs)
        {
            StringBuilder IdString = new StringBuilder(500);

            for (int count = 0; count < IDs.Count; count++)
            {
                IdString.Append(IDs[count]);
                if (count != IDs.Count - 1)
                    IdString.Append(',');
            }
            return IdString.ToString();
        }

        private void getAssetClassification(ref List<AccountPosition> listOfAccounts)
        {
            List<string> secIds = new List<string>();
            foreach (var account in listOfAccounts)
            {
                foreach (var sec in account.Positions)
                {
                    secIds.Add(sec.SecID);
                }
                string sql = SqlConstants.GET_ASSET_CLASSIFICATION;
                sql = _dbWrapper.BuildSqlInClauseQuery(secIds, ":SEC_IDS", sql);
                IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql;
                    _dbWrapper.BuildParamInClauseQuery(secIds, "SEC_IDS", cmd);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, ":a_classificationid", "47");   //getclassificationId()
                }, _context.Identity.InstitutionId);

                foreach (var row in records)
                {
                    string sec_id = row["SEC_ID"].ToString();
                    foreach (var sec in account.Positions)
                    {
                        if (sec.SecID == sec_id)
                        {
                            AssetClassification assetClassification = new AssetClassification();
                            assetClassification.AssetClassid = row["ASSET_CLASS_ID"].ToString();
                            assetClassification.AssetClassName = row["NAME"].ToString();
                            assetClassification.Pct = row["PERCENTAGE"].ToString();
                            assetClassification.Idxid = row["IDXID"].ToString();
                            sec.AssetClassifications.Add(assetClassification);
                            break;
                        }
                    }
                }
            }

            
        }

        public Boolean UpdatePortfolioAccounts(string planId, List<AccountPosition> listOfAccountsNew)
        {
            bool result = false;
            List<string> toBeDeletedAccountLists = new List<string>();
            string portfolioId = "-1";
            string targetPortfolioId = "-1";
            if (Convert.ToInt64(planId) > 0)
            {
                portfolioId = GetInitalPortfolioId(planId);
                targetPortfolioId = GetTargetPortfolioId(planId);
            }
            if (listOfAccountsNew != null && listOfAccountsNew.Count > 0 && portfolioId != "-1")
            {
                List<string> existingAcctIds = new List<string>();
                List<string> acctList = new List<string>();
                foreach (AccountPosition act in listOfAccountsNew)
                {
                    acctList.Add(act.AccountID);
                }
                existingAcctIds = GetExistingAccounts(portfolioId, acctList);
                var i = 0;
                var index = 0;
                foreach (AccountPosition account in listOfAccountsNew)
                {
                    if (existingAcctIds.Contains(account.AccountID))
                    {
                        if (account.IncludeAccountYN != "Y")
                            index += index + 2; 
                    }
                    else
                    {
                        if (account.IncludeAccountYN == "Y")
                            index += index * 3;
                    }
                }
                Action<IDbCommand>[] CommandList = new Action<IDbCommand>[index];
                foreach (AccountPosition account in listOfAccountsNew)
                {
                    string accountId = string.Empty;
                    string action = string.Empty;
                    if (portfolioId != "-1")
                    {
                        //if the account is selected, update it to the web_portfolio_account table
                        //if the acount isn't there insert it.
                        if (existingAcctIds.Contains(account.AccountID))
                        {
                            if (account.IncludeAccountYN != "Y")
                                action = "Delete";
                        }
                        else
                        {
                            if (account.IncludeAccountYN == "Y")
                                action = "Insert";
                        }
                        if (action == "Delete")
                            toBeDeletedAccountLists.Add(account.AccountID);
                        else if (action == "Insert")
                        {
                            
                            string nextPortfolioId = GetSequencePattern("WEB_PORTFOLIO_PRODUCT", "");
                            //*************************************************************************************************************
                            CommandList[i] = new Action<IDbCommand>(cmd =>
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = SqlConstants.INSERT_INVESTMENT_PORTFOLIO;
                                DatabaseWrapperHelper.AddInStringParameter(cmd, "PORTFOLIO_ID", nextPortfolioId);
                                DatabaseWrapperHelper.AddInIntParameter(cmd, "PORTFOLIO_TYPE_ID", "0");
                                DatabaseWrapperHelper.AddInIntParameter(cmd, "CREATE_BY_USER_ID", _context.Identity.BrokerId);
                            });
                            i++;
                            //*************************************************************************************************************
                            CommandList[i] = new Action<IDbCommand>(cmd =>
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = SqlConstants.INSERT_HOLDING_PORTFOLIO;
                                DatabaseWrapperHelper.AddInStringParameter(cmd, "PORTFOLIO_ID", nextPortfolioId);
                                DatabaseWrapperHelper.AddInIntParameter(cmd, "PORTFOLIO_TYPE_ID", "0");
                                DatabaseWrapperHelper.AddInStringParameter(cmd, "EXTERNAL_ID", account.AccountID);
                                DatabaseWrapperHelper.AddInIntParameter(cmd, "DATASOURCE_ID", "1");
                                DatabaseWrapperHelper.AddInIntParameter(cmd, "CREATE_BY_USER_ID", _context.Identity.BrokerId);
                            });
                            i++;
                            //*************************************************************************************************************
                            CommandList[i] = new Action<IDbCommand>(cmd =>
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = SqlConstants.INSERT_HOLDING_PORTFOLIO_COMPOSITION;
                                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOLDING_PORTFOLIO_ID", portfolioId);
                                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOLDING_COMPONENT_ID", nextPortfolioId);
                                DatabaseWrapperHelper.AddInIntParameter(cmd, "HOLDING_COMPONENT_TYPE_ID", "1");
                                DatabaseWrapperHelper.AddInStringParameter(cmd, "PRODUCT_PORTFOLIO_ID", "0");
                                DatabaseWrapperHelper.AddInIntParameter(cmd, "CREATE_BY_USER_ID", _context.Identity.BrokerId);
                            });
                            i++;
                            //*************************************************************************************************************
                        }
                        action = string.Empty;
                    }
                }
                if(toBeDeletedAccountLists.Count > 0)
                {
                    //*************************************************************************************************************
                    var deleteHoldPort = SqlConstants.DELETE_HOLDING_PORTFOLIO_COMPOSITION_ACCOUNTS;
                    deleteHoldPort = _dbWrapper.BuildSqlInClauseQuery(toBeDeletedAccountLists, ":EXTERNAL_ID", deleteHoldPort);
                    CommandList[i] = new Action<IDbCommand>(cmd =>
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = deleteHoldPort;
                        _dbWrapper.BuildParamInClauseQuery(toBeDeletedAccountLists, "EXTERNAL_ID", cmd);
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "HOLDING_PORTFOLIO_ID", portfolioId);
                    });
                    i++;
                    ////*************************************************************************************************************
                    var deletePortComp = SqlConstants.DELETE_PORTFOLIO_COMPOSITION;
                    deletePortComp = _dbWrapper.BuildSqlInClauseQuery(toBeDeletedAccountLists, ":EXTERNAL_ID", deletePortComp);
                    CommandList[i] = new Action<IDbCommand>(cmd =>
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = deletePortComp;
                        _dbWrapper.BuildParamInClauseQuery(toBeDeletedAccountLists, "EXTERNAL_ID", cmd);
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "PORTFOLIO_ID", targetPortfolioId);
                    });
                    //*************************************************************************************************************
                }

                result = _dbWrapper.ExecuteBatch(CommandList, _context.Identity.InstitutionId);
            }
            return result;
        }

        public string GetInitalPortfolioId(string planId)
        {
            string portfolioid = "-1";
            if (Convert.ToInt64(planId) > 0)
            {
                string sql = SqlConstants.GET_PORTFOLIO_ID;
                IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, ":PlanId", planId);
                }, _context.Identity.InstitutionId);

                foreach (var row in records)
                {
                    portfolioid = row["id"].ToString();
                }

            }
            return portfolioid;
        }

        public string GetTargetPortfolioId(string planId)
        {
            string portfolioid = "-1";
            if (Convert.ToInt64(planId) > 0)
            {
                string sql = SqlConstants.GET_TARGET_PORTFOLIO_ID;
                IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, ":PlanId", planId);
                }, _context.Identity.InstitutionId);

                foreach (var row in records)
                {
                    portfolioid = row["id"].ToString();
                }
            }
            return portfolioid;
        }

        private List<string> GetExistingAccounts(string portfolioId, List<string> actIdList)
        {
            List<string> selAccts = new List<string>();
            string sql = SqlConstants.GET_EXISTING_ACCTS;
            sql = _dbWrapper.BuildSqlInClauseQuery(actIdList, ":acct_id", sql);
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                _dbWrapper.BuildParamInClauseQuery(actIdList, "acct_id", cmd);
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":PORTFOLIOID", portfolioId);   //getclassificationId()
            }, _context.Identity.InstitutionId);

            foreach (var row in records)
            {
                selAccts.Add(row["ACCOUNT_ID"].ToString());
            }
            return selAccts;       
        }

        private  string GetSequencePattern(string tableName, string recordType)
        {
            string seq = string.Empty;
            string sql = SqlConstants.GET_NEXT_PORTFOLIO_ID;
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":Inst_ID", _context.Identity.InstitutionId);
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":Table_Name", tableName);
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":Record_Type", recordType);
            }, _context.Identity.InstitutionId);

            foreach (var row in records)
            {
                seq = row["SeqValue"] != null ? row["SeqValue"].ToString() : "";
            }
            return seq;
        }

        public Boolean LockExcludePositions(string planId, LockExclAcct accountPosition)
        {
            Boolean result = false;
            if (accountPosition != null && accountPosition.AccountID != "-1")
            {
                string accountID = accountPosition.AccountID;
                //AccountDetail accountDetail = accountPosition.AccountDetail;
                string lockedSecId = string.Empty;
                string excludedSecId = string.Empty;
                string portfolioId = GetInitalPortfolioId(planId);

                var index = accountPosition.Positions.Count;
                foreach (LockExclPosition opm in accountPosition.Positions)
                {
                    //Check if the sec_id excluded in web_position_filter table
                    if (opm.ExcludeYN == true || opm.LockedYN == true)
                    {
                        index++;
                    }
                }
                var i = 0;
                Action<IDbCommand>[] CommandList = new Action<IDbCommand>[index];
                foreach (LockExclPosition opm in accountPosition.Positions)
                {
                    CommandList[i] = new Action<IDbCommand>(cmd => {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = SqlConstants.DELETE_FILTER_SECURITY;
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", accountPosition.AccountID);
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "PORTFOLIO_ID", portfolioId);
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "SEC_ID", opm.SecID);
                    });
                    i++;
                }

                foreach (LockExclPosition opm in accountPosition.Positions)
                {
                    //Check if the sec_id excluded in web_position_filter table
                    if (opm.ExcludeYN == true || opm.LockedYN == true)
                    {
                        //exclude the security
                        CommandList[i] = new Action<IDbCommand>(cmd =>
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = SqlConstants.INSERT_FILTERED_SECURITY;
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "PORTFOLIO_ID", portfolioId);
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", accountPosition.AccountID);
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "SEC_ID", opm.SecID);
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "LOCKED_YN", (opm.LockedYN == true ? "Y" : "N"));
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "EXCLUDE_YN", (opm.ExcludeYN == true ? "Y" : "N"));
                        });
                        i++;
                    }
                }
                result = _dbWrapper.ExecuteBatch(CommandList, _context.Identity.InstitutionId);
            }
            return result;
        }
        public List<AccountType> GetAccountTypes()
        {
            List<AccountType> accountTypes = new List<AccountType>();
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_ACCOUNT_TYPES;
            }, _context.Identity.InstitutionId);
            foreach (var row in records)
            {
                AccountType accountType = new AccountType();
                accountType.AccountTypeId = row["ACCOUNT_TYPE_ID"] is DBNull ? 0 : Convert.ToInt64(row["ACCOUNT_TYPE_ID"]);
                accountType.AccountTaxStatusId = row["TAX_STATUS_ID"] is DBNull ? 0 : Convert.ToInt64(row["TAX_STATUS_ID"]);
                accountType.AccountTypeDesc = row["ACCOUNT_TYPE_DESCR"].ToString();
                accountType.TaxTypeDesc = row["ACCOUNT_TAX_TYPE_DESC"].ToString();
                accountTypes.Add(accountType);
            }
            return accountTypes;
        }
        public string CreateAccountBasicInfo(AccountBasicInfoRequest account)
        {
            long accountId =  Convert.ToInt64(_dbWrapper.ExecuteScalar(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_SEQUENCE_ACCOUNT_ID;
            }, _context.Identity.InstitutionId));
            string  dataSourceID = GetDataSourceID();
            string newActId = "";

            List<Action<IDbCommand>> configureCommandList = new List<Action<IDbCommand>>();
            if (accountId != -1 && long.TryParse(account.HouseholdId,out long householdId))
            {
                //Create Account - Web_Account
                newActId = string.Format("W-{0}", accountId.ToString());
                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.CREATE_ACCOUNT_INFO;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", newActId);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_NAME", account.AccountName);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "TAX_TREATMENT", ((int)account.TaxTreatment).ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_TYPE_ID", account.AccountTypeId.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "FILING_STATUS", ((int)account.OwnershipType).ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_DETAIL", account.AccountDetail == AccountDetail.SECURITY ? "Y" : "N");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "INTERNAL_MANAGED_YN", "N");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_NUMBER", account.AccountNumber);

                }));

                //Create Investor
                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.CREATE_ACCOUNT_INVESTOR;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "INVESTOR_ID", account.PrimaryOwnerId);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", newActId);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "OWNERSHIP_TYPE", "1");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "DATASOURCE_ID", dataSourceID);
                }));

                if (!string.IsNullOrEmpty(account.SecondaryOwnerId) && long.Parse(account.SecondaryOwnerId) > 0)
                {
                    //Create Secondary Investor
                    configureCommandList.Add(new Action<IDbCommand>(cmd =>
                    {
                        cmd.CommandText = SqlConstants.CREATE_ACCOUNT_INVESTOR;
                        DatabaseWrapperHelper.AddInStringParameter(cmd,  "INVESTOR_ID", account.SecondaryOwnerId);
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", newActId);
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "OWNERSHIP_TYPE", "2");
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "DATASOURCE_ID", dataSourceID);
                    }));                   
                }                
                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.CREATE_ACCOUNT_HOUSEHOLD;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "HOUSEHOLD_ID", account.HouseholdId);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", newActId);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "DATASOURCE_ID", dataSourceID);
                }));
                bool result = _dbWrapper.ExecuteBatch(configureCommandList.ToArray(), _context.Identity.InstitutionId);
            }
            return newActId;
        }
        public AssetClassAccount GetAllAssetPositions()
        {
           List<OpenPosition> listOfPosition = new List<OpenPosition>();            
            ACAccountAlgorithm acAccountBuilder = new ACAccountAlgorithm(_brokerMgrRepository);
            return acAccountBuilder.Calculate(listOfPosition);
        }

        public bool DeleteAccount(string accountId)
        {

            var executeResult = _dbWrapper.ExecuteScalar(cmd =>
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = SqlConstants.DELETE_ACCOUNT;
                DatabaseWrapperHelper.AddInStringParameter(cmd, "a_vAccount_id", accountId);
                DatabaseWrapperHelper.AddOutIntParameter(cmd, "A_UTSTATUS");
                DatabaseWrapperHelper.AddOutStringParameter(cmd, "A_UTSTATUSMSG", 2000);
            }, _context.Identity.InstitutionId);
            return true;
            
        }

        public bool SaveAssetAccountPositions(SaveAssetClassPositionsRequest account)
        {
            long lPositionID = -1;
            long lTransactionID;
            double dAmount;
            double dBalance = account.Balance;
            List<Action<IDbCommand>> configureCommandList = new List<Action<IDbCommand>>();
            ////Update Cash
            configureCommandList.Add(new Action<IDbCommand>(cmd =>
            {
                cmd.CommandText = SqlConstants.update_Account_CashBalance;
                DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", account.AccountId);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "CASH_COMPONENT", "0");                
            }));
            //Delete All Transactions
            configureCommandList.Add(new Action<IDbCommand>(cmd =>
            {
                cmd.CommandText = SqlConstants.deleteAllTransactions;
                DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", account.AccountId);
            }));
            //Delete All Positions
            configureCommandList.Add(new Action<IDbCommand>(cmd =>
            {
                cmd.CommandText = SqlConstants.deleteAllPositions;
                DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", account.AccountId);
            }));
            foreach (SaveAssetClassPosition acPosition in account.ACPositions)
            {
                dAmount = (acPosition.Pct / 100.00) * dBalance;
               // lPositionID = -1;
                if (dAmount > 0)
                {
                    
                        configureCommandList.Add(new Action<IDbCommand>(cmd =>
                        {
                            lPositionID = Convert.ToInt64(_dbWrapper.ExecuteScalar(cmd1 =>
                            {
                                cmd1.CommandText = SqlConstants.getNewPositionID;
                            }, _context.Identity.InstitutionId));
                            dAmount = (acPosition.Pct / 100.00) * dBalance;
                            cmd.CommandText = SqlConstants.createNewAccountPosition;
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", account.AccountId);
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "POSITION_ID", lPositionID.ToString());
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "POSITION_TERM", ((long)PositionTerm.LONG).ToString());
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "QUANTITY", dAmount.ToString());
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "PURCHASE_PRICE", "1");
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "PURCHASE_DATE", DateTime.Now.ToString("MM/dd/yyyy"));
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "MKT_VAL_ENTRY_MODE", ((long)MarketValueEntry.MARKET_VALUE).ToString());
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "SEC_ID", acPosition.assetClass.SecId);
                        }));
                       
                        if(lPositionID != -1)
                        //Create Transaction - Web_Transaction
                        configureCommandList.Add(new Action<IDbCommand>(cmd =>
                        {
                            lTransactionID = Convert.ToInt64(_dbWrapper.ExecuteScalar(cmd1 =>
                            {
                                cmd1.CommandText = SqlConstants.getNewTransactionID;
                            }, _context.Identity.InstitutionId));
                            cmd.CommandText = SqlConstants.createNewAccountTransaction;
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "POSITION_ID", lPositionID.ToString());
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "TRANSACTION_ID", lTransactionID.ToString());
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "TRANSACTION_TYPE_ID", ((long)(TransactionType.BUY)).ToString());
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "PURCHASE_DATE", DateTime.Now.ToString("MM/dd/yyyy"));
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "PURCHASE_PRICE", "1");
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "QUANTITY", dAmount.ToString());
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "COMMISSION", "0");
                        }));
                    

                }
            }

            bool result = _dbWrapper.ExecuteBatch(configureCommandList.ToArray(), _context.Identity.InstitutionId);
            return result;

        }

        public  bool UpdateAccountBasicsInfo(AccountBasicInfoRequest account)
        {
            string dataSourceID = GetDataSourceID();
            List<Action<IDbCommand>> configureCommandList = new List<Action<IDbCommand>>();
            ////Update Cash
            configureCommandList.Add(new Action<IDbCommand>(cmd =>
            {
                cmd.CommandText = SqlConstants.UPDATE_ACCOUNT_INFO;
                DatabaseWrapperHelper.AddInStringParameter(cmd,"ACCOUNT_ID", account.AccountID);
                DatabaseWrapperHelper.AddInStringParameter(cmd,"ACCOUNT_NAME", account.AccountName);
                DatabaseWrapperHelper.AddInStringParameter(cmd,"TAX_TREATMENT", ((int)account.TaxTreatment).ToString());
                DatabaseWrapperHelper.AddInStringParameter(cmd,"ACCOUNT_TYPE_ID", account.AccountTypeId.ToString());
                DatabaseWrapperHelper.AddInStringParameter(cmd,"OWNERSHIP_TYPE", ((int)account.OwnershipType).ToString());
                DatabaseWrapperHelper.AddInStringParameter(cmd,"ACCOUNT_NUMBER", account.AccountNumber);
                //DatabaseWrapperHelper.AddInStringParameter(cmd,"CASH_COMPONENT", account.CashComponent.ToString());
                //DatabaseWrapperHelper.AddInStringParameter(cmd,"BALANCE", account.MarketValue.ToString());
            }));
            //Delete investor
            configureCommandList.Add(new Action<IDbCommand>(cmd =>
            {
                cmd.CommandText = SqlConstants.DELETE_INVESTOR;
                DatabaseWrapperHelper.AddInStringParameter(cmd,"ACCOUNT_ID", account.AccountID);
            }));
            //Create investors
            configureCommandList.Add(new Action<IDbCommand>(cmd =>
            {
                cmd.CommandText = SqlConstants.CREATE_ACCOUNT_INVESTOR;
                DatabaseWrapperHelper.AddInStringParameter(cmd, "INVESTOR_ID", account.PrimaryOwnerId);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", account.AccountID);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "OWNERSHIP_TYPE", "1");
                DatabaseWrapperHelper.AddInStringParameter(cmd, "DATASOURCE_ID", dataSourceID);
            }));

            if (!string.IsNullOrEmpty(account.SecondaryOwnerId) && long.Parse(account.SecondaryOwnerId) > 0)
            {
                //Create Secondary Investor
                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.CREATE_ACCOUNT_INVESTOR;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "INVESTOR_ID", account.SecondaryOwnerId);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", account.AccountID);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "OWNERSHIP_TYPE", "2");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "DATASOURCE_ID", dataSourceID);
                }));
            }
            
            bool result = _dbWrapper.ExecuteBatch(configureCommandList.ToArray(), _context.Identity.InstitutionId);
            return result;

        }

        private string GetDataSourceID()
        {
            return _brokerMgrRepository.IsBackOfficeInstitution() ? "2" : "1";
        }


        public bool SaveSeurityAccountPositions(SaveSecurityPositionsRequest account)
        {
            long lPositionID = -1;
            long lTransactionID = -1;
            List<Action<IDbCommand>> configureCommandList = new List<Action<IDbCommand>>();

            configureCommandList.Add(new Action<IDbCommand>(cmd =>
            {
                cmd.CommandText = SqlConstants.update_Account_CashBalance;
                DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", account.AccountID);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "CASH_COMPONENT", account.CashComponent.ToString());
            }));
            //Delete All Transactions
            configureCommandList.Add(new Action<IDbCommand>(cmd =>
            {
                cmd.CommandText = SqlConstants.deleteAllTransactions;
                DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", account.AccountID);
            }));
            //Delete All Positions
            configureCommandList.Add(new Action<IDbCommand>(cmd =>
            {
                cmd.CommandText = SqlConstants.deleteAllPositions;
                DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", account.AccountID);
            }));
            foreach (SaveSecurityPosition pos in account.Positions)
            {
                //Create Position - Web_Position
                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    lPositionID = Convert.ToInt64(_dbWrapper.ExecuteScalar(cmd1 =>
                    {
                        cmd1.CommandText = SqlConstants.getNewPositionID;
                    }, _context.Identity.InstitutionId));
                    //dAmount = (acPosition.Pct / 100.00) * dBalance;
                    cmd.CommandText = SqlConstants.createNewAccountPosition;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", account.AccountID);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "POSITION_ID", lPositionID.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "POSITION_TERM", ((long)pos.PositionTerm).ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "QUANTITY", pos.Qty.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PURCHASE_PRICE", pos.CurrentPrice.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PURCHASE_DATE", pos.OpenDate.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "MKT_VAL_ENTRY_MODE", ((long)pos.Marketalue_entry_mode).ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "SEC_ID", pos.SecID);
                }));



                //Create Transaction - Web_Transaction
                if (lPositionID != -1)
                {
                    //Create Transaction - Web_Transaction
                    configureCommandList.Add(new Action<IDbCommand>(cmd =>
                    {
                        lTransactionID = Convert.ToInt64(_dbWrapper.ExecuteScalar(cmd1 =>
                        {
                            cmd1.CommandText = SqlConstants.getNewTransactionID;
                        }, _context.Identity.InstitutionId));
                        cmd.CommandText = SqlConstants.createNewAccountTransaction;
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "POSITION_ID", lPositionID.ToString());
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "TRANSACTION_ID", lTransactionID.ToString());
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "TRANSACTION_TYPE_ID", ((long)(pos.PositionTerm)).ToString());
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "PURCHASE_DATE", pos.OpenDate);
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "PURCHASE_PRICE", pos.CurrentPrice.ToString());
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "QUANTITY", pos.Qty.ToString());
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "COMMISSION", pos.Commissions.ToString());
                    }));
                }
            }

            bool result = _dbWrapper.ExecuteBatch(configureCommandList.ToArray(), _context.Identity.InstitutionId);
            return result;

        }
                   
             
            

            
           



    }
}
