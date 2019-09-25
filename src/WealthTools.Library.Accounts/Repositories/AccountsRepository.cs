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

namespace WealthTools.Library.Accounts.Repositories
{
    public class AccountsRepository : IAccountsRepository
    {
        IDatabaseWrapper _dbWrapper;
        IContext _context;
        IConnectionManager _connectionManager;

        public AccountsRepository(IDatabaseWrapper dbWrapper, IContext context)
        {
            _dbWrapper = dbWrapper;
            _context = context;
        }

        public List<AccountPosition> GetAccountSummary(string householdID, string planId)
        {
            List<AccountPosition> listOfAccounts = new List<AccountPosition>();
            int _householdID = -1;
            if (householdID != string.Empty) { 
                _householdID = Int32.Parse(householdID);
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
                account.Account_name = row["ACCOUNT_NAME"].ToString();
                account.Account_number = row["ACCOUNT_NUM"].ToString();
                account.Attribute2 = row["ATTR2"].ToString();
                account.Display_type_id = row["DISPLAY_TYPE_ID"].ToString();
                account.InternalYN = row["INTERNAL_YN"].ToString();
                account.ProgramID = row["PROGRAM_ID"].ToString();
                //account.TestAccount = Convert.ToBoolean(row["TEST_YN"]);
                account.Error_message = "";
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
                acct.Account_name = row["ACCOUNT_NAME"].ToString();
                acct.IrsName = row["IRS_NAME"].ToString();
                acct.Owner_name = row["OWNER_NAME"].ToString();
                acct.Account_number = row["ACCOUNT_NUM"].ToString();
                acct.ATTR2 = row["ATTR2"].ToString();
                acct.Program_id = row["PROGRAM_ID"].ToString();
                acct.Cash_component = row["CASH_COMPONENT"] is DBNull ? 0 : Convert.ToDouble(row["CASH_COMPONENT"]);
                acct.CurrencyCode = row["CURRENCY_CODE"].ToString();
                acct.Rate = row["RATE"] is DBNull ? 0.0 : Convert.ToDouble(row["RATE"]);
                acct.Long_balance = row["LONG_MARKET_VALUE"] is DBNull ? 0 : Convert.ToDouble(row["LONG_MARKET_VALUE"]);
                acct.Short_balance = row["SHORT_MARKET_VALUE"] is DBNull ? 0 : Convert.ToDouble(row["SHORT_MARKET_VALUE"]);
                acct.Market_value = row["MARKET_VALUE"] is DBNull ? 0 : Convert.ToDouble(row["MARKET_VALUE"]);
                acct.InternalValueDBField = row["INTERNAL_MANAGED_YN"].ToString();
                acct.ManualDBField = row["MANUAL_FLAG"].ToString() != "N";
                acct.Tax_treatmentDBField = row["TAX_STATUS_ID"] is DBNull ? 0 : Convert.ToInt64(row["TAX_STATUS_ID"]);
                acct.Nature_of_acct = row["NATURE_OF_ACCOUNT"].ToString();
                acct.Ownership_typeDBField = row["FILING_STATUS_ID"] is DBNull ? 0 : Convert.ToInt64(row["FILING_STATUS_ID"]);
                acct.Account_type_id = row["ACCOUNT_TYPE_ID"] is DBNull ? 0 : Convert.ToInt64(row["ACCOUNT_TYPE_ID"]);
                acct.Primary_owner_id = row["PRIMARY_OWNER_ID"].ToString();
                acct.Acct_owner_id = row["ACCT_OWNER_ID"].ToString();
                acct.Secondary_owner_id = row["SECONDARY_OWNER_ID"].ToString();
                acct.AccountDetailDBField = row["ACCOUNT_DETAIL"].ToString();
                acct.LastUpdatedDate = row["last_modified_date"].ToString();
                acct.Portfolio_Acct_Type = row["PORTFOLIO_ACCT_TYPE"].ToString();
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
                position.Is_market_value_presetYN = row["MARKET_VALUE_PRESET"].ToString();
                position.Market_value_entry_modeDB = row["MKT_VAL_ENTRY_MODE"].ToString();
                position.Open_date = row["PURCHASE_DATE"].ToString();
                position.Open_price = row["PURCHASE_PRICE"] is DBNull ? 0 : Convert.ToDouble(row["PURCHASE_PRICE"]);
                position.Position_termDB = row["POSITION_TERM"].ToString();
                position.Pos_id = row["POSITION_ID"] is DBNull ? 0 : Convert.ToInt64(row["POSITION_ID"]);
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
                                position.Current_price = row["CURRENT_PRICE"] is DBNull ? 0.0 : Convert.ToDouble(row["CURRENT_PRICE"]);
                                position.Pricing_date = row["AS_OF_DATE"].ToString();
                                position.Security_Detail.Cusip = row["CUSIP"].ToString();
                                position.Security_Detail.Sec_sedol = row["SECURITY_SEDOL"].ToString();
                                position.Security_Detail.Sec_name = row["NAME"].ToString();
                                position.Security_Detail.Sec_type = row["SEC_TYPE"].ToString();
                                position.Security_Detail.Sub_type = row["SUB_TYPE"].ToString();
                                position.Security_Detail.Sec_symbol = row["TICKER"].ToString();
                                position.Security_Detail.Price_factor = row["PRICE_FACTOR"] is DBNull ? 0 : Convert.ToDouble(row["PRICE_FACTOR"]);
                                position.Security_Detail.Price_ADJ_factor = row["PRICE_ADJ_FACTOR"] is DBNull ? 0 : Convert.ToDouble(row["PRICE_ADJ_FACTOR"]);
                                position.Security_Detail.Currency_Code = row["CURRENCY_CODE"].ToString();
                                position.Security_Detail.Currency_Conv_Factor = row["RATE"] is DBNull ? 0 : Convert.ToDouble(row["RATE"]);
                                position.Security_Detail.Sec_status = row["STATUS"].ToString();
                                position.Security_Detail.FileExists = row["FILEEXISTS"] is DBNull ? false : Convert.ToBoolean(row["FILEEXISTS"]);
                                if (position.Current_price < PriceHelper.ZeroPrice)
                                {
                                    double price = PriceHelper.DefaultPrice;
                                    if(position.Security_Detail.Sec_type != null)
                                    if (position.Security_Detail.Sec_type.CompareTo("FI") == 0)
                                        price *= 100.0;
                                    position.Current_priceDB = price;
                                }
                                //For AC level securities, the as of date is null. Hence, update the Pricing_date
                                //with the last modified date of the account.
                                if (position.Security_Detail.Sec_type == "AC")
                                {
                                    if (position.Current_price < PriceHelper.ZeroPrice)
                                        position.Current_priceDB = PriceHelper.DefaultPrice;
                                    position.Pricing_date = account.Last_modified_date;
                                }
                                // set HasFactSheet flag
                                // position.Security_Detail.HasFactSheet = GetFactSheetInfo(context, position);
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
                    if (position != null && (position.Security_Detail.Sec_status == "" || position.Security_Detail.Sec_status == "IN"))
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
                    account.Market_value = AccountBalanceTraits.CalculateBalance(account);
                    account.Long_balance = AccountBalanceTraits.CalculateLongBalance(account);
                    account.Short_balance = AccountBalanceTraits.CalculateShortBalance(account);
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
                    if ((position.Position_term == PositionBase.tPositionTerm.LONG)
                            && (position.Market_value > 0))
                    {
                        accountLongBalance += position.Market_value;
                    }
                }
                account.Sec_Count = noOfSecurity;
                account.Long_balance = accountLongBalance;
                account.Market_value = account.Long_balance
                                                        + ((account.Cash_component > 0) ? account.Cash_component : 0);

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
                                apIterator.Include_account_yn = "Y";
                            string accountDetail = apIterator.Account_detail;
                            if (!accountDetail.Equals(AccountBasics.tAccountDetail.INVALID_ACCOUNTDETAIL))
                            {
                                foreach (PositionBase opm in apIterator.Positions)
                                {
                                    opm.Security_Detail.Sec_name = (opm.Security_Detail.Sec_name == string.Empty) ? "--" : opm.Security_Detail.Sec_name;

                                    //if the market value for shot postion is not negative, make it negative
                                    if ((opm.Position_term == PositionBase.tPositionTerm.SHORT) && (opm.Market_value > 0))
                                        opm.Market_value = (opm.Market_value * -1.0);
                                    if ((opm.Position_term == PositionBase.tPositionTerm.SHORT) && (opm.Qty > 0))
                                        opm.Qty = (opm.Qty * -1.0);
                                    if (positionCollection.Count > 0)
                                    {
                                        if (postion.SecId == opm.SecID && postion.AccountId == opm.AccountID)
                                        {
                                            opm.Locked_yn = postion.lockedYN;
                                            opm.Exclude_yn = postion.ExcludeYN;

                                            if (postion.ExcludeYN == true)//subtract the market value
                                            {
                                                if (opm.Position_term.Equals(PositionBase.tPositionTerm.LONG) && opm.Market_value > 0)
                                                {
                                                    apIterator.Long_balance = apIterator.Long_balance - opm.Market_value;
                                                    apIterator.Market_value = apIterator.Market_value - opm.Market_value;
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
                        if (account.Include_account_yn != "Y")
                            index += index + 2; 
                    }
                    else
                    {
                        if (account.Include_account_yn == "Y")
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
                            if (account.Include_account_yn != "Y")
                                action = "Delete";
                        }
                        else
                        {
                            if (account.Include_account_yn == "Y")
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

        public Boolean SavePositions(string planId, AccountPosition accountPosition)
        {
            Boolean result = false;
            if (accountPosition != null && accountPosition.AccountID != "-1")
            {
                string accountID = accountPosition.AccountID;
                string accountDetail = accountPosition.Account_detail;
                string lockedSecId = string.Empty;
                string excludedSecId = string.Empty;
                string portfolioId = GetInitalPortfolioId(planId);

                var index = accountPosition.Positions.Count;
                foreach (PositionBase opm in accountPosition.Positions)
                {
                    //Check if the sec_id excluded in web_position_filter table
                    if (opm.Exclude_yn == true || opm.Locked_yn == true)
                    {
                        index++;
                    }
                }
                var i = 0;
                Action<IDbCommand>[] CommandList = new Action<IDbCommand>[index];
                foreach (PositionBase opm in accountPosition.Positions)
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

                foreach (PositionBase opm in accountPosition.Positions)
                {
                    //Check if the sec_id excluded in web_position_filter table
                    if (opm.Exclude_yn == true || opm.Locked_yn == true)
                    {
                        //exclude the security
                        CommandList[i] = new Action<IDbCommand>(cmd =>
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = SqlConstants.INSERT_FILTERED_SECURITY;
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "PORTFOLIO_ID", portfolioId);
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "ACCOUNT_ID", accountPosition.AccountID);
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "SEC_ID", opm.SecID);
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "LOCKED_YN", (opm.Locked_yn == true ? "Y" : "N"));
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "EXCLUDE_YN", (opm.Exclude_yn == true ? "Y" : "N"));
                        });
                        i++;
                    }
                }
                result = _dbWrapper.ExecuteBatch(CommandList, _context.Identity.InstitutionId);
            }
            return result;
        }

    }
}
