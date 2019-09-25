using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WealthTools.Common.DatabaseConnection;
using WealthTools.Common.DatabaseWrapper;
using WealthTools.Common.DatabaseWrapper.Interfaces;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.Contacts.Interfaces;
using WealthTools.Library.Contacts.Models;


namespace WealthTools.Library.Contacts.Repositories
{
    public class ContactsRepository : IContactsRepository
    {
        readonly IDatabaseWrapper _dbWrapper;
        readonly IContext _context;        

        public ContactsRepository(IDatabaseWrapper dbWrapper, IContext context)
        {
            _dbWrapper = dbWrapper;
            _context = context;            
        }        

        public List<SearchResult> SearchAllContacts(SearchParameters searchParameters)
        {
            List<SearchResult> resultList = new List<SearchResult>();
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (searchParameters.Searchby == SearchBy.ACCOUNTNUMBER)
                {
                    cmd.CommandText = SqlConstants.GET_ALL_CONTACTSBY_ACCOUNTNO;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VACCTNO", HandleSpecialCharacters(searchParameters.AccountNumber));
                }
                else if (searchParameters.Searchby == SearchBy.REPCODE)
                {
                    cmd.CommandText = SqlConstants.GET_ALL_CONTACTSBY_REPCODE;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VREPCODE", HandleSpecialCharacters(searchParameters.RepCode));
                    
                }
                else //default SearchBy DEMOGRAPHICS
                {
                    cmd.CommandText = SqlConstants.GET_ALL_CONTACTSBY_DEMOGRAPHICS;
                    SetDemographicParameters(cmd, searchParameters);
                }
                //CommonParameters
                SetCommonParameters(cmd, searchParameters);
                //Output Parameters
                DatabaseWrapperHelper.AddOutCursorParameter(cmd, "A_RCCONTACTLIST");
                DatabaseWrapperHelper.AddOutIntParameter(cmd, "A_UTSTATUS");
                DatabaseWrapperHelper.AddOutStringParameter(cmd, "A_UTSTATUSMSG", 2000);
            }, _context.Identity.InstitutionId);
            foreach (var row in records)
            {
                SearchResult result = new SearchResult();
                result.HouseholdID = row["HOUSEHOLD_ID"].ToString();                
                if (!String.IsNullOrEmpty(result.HouseholdID) && Convert.ToInt32(result.HouseholdID) > 0 && !resultList.Any(p => p.HouseholdID == result.HouseholdID))
                {
                    result.Type = row["TYPE"].ToString();
                    //Get Contact Details               
                    result.Persons = GetContactsForHousehold(result.HouseholdID);
                    resultList.Add(result);
                }
            }
            return resultList;
        }

        public List<Contact> GetContactsForHousehold(string householdId)
        {
            List<Contact> contactList = new List<Contact>();
            bool isValidHH = long.TryParse(householdId, out long hhId);
            if (!isValidHH) return contactList;
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SqlConstants.GET_PERSONS_FOR_HH;
                DatabaseWrapperHelper.AddInIntParameter(cmd, "household_id", householdId);
            }, _context.Identity.InstitutionId);
            foreach (var row in records)
            {
                Contact contact = new Contact();
                contact.HouseholdId = householdId;
                contact.InvestorId = row["PERSON_ID"].ToString();
                contact.RelationShipType = (RelationShipType)row["RELATIONSHIP_TYPE"];
                contact.FirstName = row["first_name"].ToString();
                contact.LastName = row["last_name"].ToString();
                contact.MiddleName = row["mid_initial"].ToString();
                contact.Suffix = row["suffix"].ToString();
                contact.PersonalEmail = row["HOME_EMAIL"].ToString();
                contact.Phone = row["MOBILE_PHONE"].ToString();
                contact.HomePhone = row["HOME_PHONE"].ToString();
                contact.AddressLine1 = row["mail_addr1"].ToString();
                contact.AddressLine2 = row["mail_addr2"].ToString();
                contact.City = row["mail_city"].ToString();
                contact.State = row["mail_state"].ToString();
                contact.Country = row["mail_country"].ToString();
                contact.Zip = row["mail_postal_code"].ToString();
                contactList.Add(contact);
            }
            return contactList;
        }

        public SearchResult CreateProspectHousehold(List<Contact> contactsListColl)
        {
            //if (_isBackOffice)
            //    household = hhManager.CreateHouseholdForTeams(context, household, null, null, null);
            //else
            //    household = hhManager.CreateHouseholdForBroker(context, brokerCol, household);
            SearchResult res =  CreateHousehold(contactsListColl);
            InsertAllATTR2IntoHH(res.HouseholdID);
            foreach (Contact contact in res.Persons)
            {
                if (long.TryParse( contact.InvestorId, out long investorId) && investorId > 0 && contact.RelationShipType == RelationShipType.HEAD)
                {
                    UpdateRecentContact(res.HouseholdID, contact.InvestorId);
                    break;
                }
            }
            return res;
        }
        private void SetCommonParameters(IDbCommand cmd, SearchParameters searchParameters)
        {
            DatabaseWrapperHelper.AddInLongParameter(cmd, "A_NBROKER_ID", _context.Identity.BrokerId);
            DatabaseWrapperHelper.AddInIntParameter(cmd, "A_NSTARTROW", searchParameters.StartRow);
            DatabaseWrapperHelper.AddInIntParameter(cmd, "A_NCOUNT", searchParameters.Count);
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_NSORTBYCOL", "");
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VASC_YN", (searchParameters.IsSortAsc) ? "Y" : "N");
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VTOTCNTONLY_YN",  "N");
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_SUPPACCT",  "N");
        }

        private void SetDemographicParameters(IDbCommand cmd, SearchParameters searchParameters)
        {
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VLNAME", HandleSpecialCharacters(searchParameters.LastName));
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VFNAME", HandleSpecialCharacters(searchParameters.FirstName));
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VSSN_TIN", string.Empty);
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VCITY", HandleSpecialCharacters(searchParameters.City));
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VSTATE", HandleSpecialCharacters(searchParameters.State));
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VZIP", HandleSpecialCharacters(searchParameters.Zip));

        }

        private string HandleSpecialCharacters(string inputString)
        {
            string modifiedString = inputString;
            if (inputString != null && inputString != string.Empty && inputString.IndexOf("'") >= 0)
            {
               modifiedString = inputString.Replace("'", "''");                
            }
            return modifiedString;
        }

        //CreateHouseholdForTeams
        private SearchResult CreateHousehold(List<Contact> contacts)
        {
            SearchResult result = new SearchResult();
            Dictionary<long, long> contactDic = new Dictionary<long, long>();
            string hhName = "Client Household";
            //Get next householdid
            int householdId = Convert.ToInt32(_dbWrapper.ExecuteScalar(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_NEXT_SEQUENCE_HOUSEHOLDID;
            }, _context.Identity.InstitutionId));           
         

            //calcualte the household name from the primary contact details
            foreach (Contact cp in contacts)
            {
                if (cp.RelationShipType == RelationShipType.HEAD)
                {
                    hhName = cp.FirstName + " " + cp.LastName;
                    //name = hhName.Trim();
                    break;
                }
            }

            var executeResult = _dbWrapper.Execute(cmd =>
            {
                cmd.CommandText = SqlConstants.CREATE_HOUSEHOLD;
                DatabaseWrapperHelper.AddInIntParameter(cmd, "householdId", householdId.ToString());
                DatabaseWrapperHelper.AddInStringParameter(cmd, "householdName", hhName);
                DatabaseWrapperHelper.AddInIntParameter(cmd, "householdTypeId", ((int)HouseholdType.PROSPECT).ToString());
            }, _context.Identity.InstitutionId);            

            // insert into web_investor
            foreach (Contact contact in contacts)
            {
               CreateContact(contact, householdId);                
            }
            
            result.HouseholdID = householdId.ToString();
            result.Name = hhName;
            result.Type = "PROSPECT";
            result.Persons = contacts;
            return result;
        }

        public  bool CreateContact(Contact newCt, long householdId)
        {
            int investorId = Convert.ToInt32(_dbWrapper.ExecuteScalar(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_NEXT_SEQUENCE_INVESTORID;
            }, _context.Identity.InstitutionId));           

            // temp solution : NFS data is longer than 30 bytes
            //if (newCt.FirstName.Length > 30)
            //    newCt.FirstName = newCt.FirstName.Substring(0, 30);
            //if (newCt.LastName.Length > 30)
            //    newCt.LastName = newCt.LastName.Substring(0, 30);

            var executeResult = _dbWrapper.Execute(cmd =>
            {
                cmd.CommandText = SqlConstants.CREATE_WEB_USER;
                DatabaseWrapperHelper.AddInLongParameter(cmd, "INVESTOR_ID", investorId.ToString());
                DatabaseWrapperHelper.AddInLongParameter(cmd, "INSTITUTION_ID", _context.Identity.InstitutionId);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "USERNAME", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "PASSWORD", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "PASSWORD_HINT", string.Empty);
                DatabaseWrapperHelper.AddInDateTimeParameter(cmd, "CREATE_DATE", DateTime.Now.Date);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "STATUS", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "USER_TYPE_ID", "2");
                DatabaseWrapperHelper.AddInStringParameter(cmd, "INSTITUTION_ID_LOGIN", string.Empty);
            }, _context.Identity.InstitutionId);

            executeResult = _dbWrapper.Execute(cmd =>
            {
                cmd.CommandText = SqlConstants.CREATE_WEB_INVESTOR;
                DatabaseWrapperHelper.AddInLongParameter(cmd, "INVESTOR_ID", investorId.ToString());
                DatabaseWrapperHelper.AddInStringParameter(cmd, "PREFIX",String.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "SUFFIX", String.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "FIRST_NAME", newCt.FirstName);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "MID_INITIAL", newCt.MiddleName);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "LAST_NAME", newCt.LastName);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "BIRTHDATE", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "SSN_TIN", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "MARK_DELETED_YN", "N");
                DatabaseWrapperHelper.AddInStringParameter(cmd, "GENDER", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "INVESTOR_NUM", investorId.ToString());
                DatabaseWrapperHelper.AddInDateTimeParameter(cmd, "CREATE_DATE", DateTime.Now.Date);
                DatabaseWrapperHelper.AddInDateTimeParameter(cmd, "LAST_ACCESS_DATE", DateTime.Now.Date);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "MARITAL_STATUS", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "SAME_AS_MAIL_YN", string.Empty);
                //Home and Mail correspondence
                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOME_PHONE", newCt.HomePhone);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "FAX", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":EMAIL_ADDR", newCt.PersonalEmail);
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":PHONE_MOBILE", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":RELATIONSHIP", string.Empty);
                //Home and Mail Address
                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOME_ADDR1", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOME_ADDR2", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOME_CITY", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOME_STATE", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOME_POSTAL_CODE", string.Empty);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOME_COUNTRY", string.Empty);
                
                DatabaseWrapperHelper.AddInStringParameter(cmd, "MAIL_ADDR1",  newCt.AddressLine1 );
                DatabaseWrapperHelper.AddInStringParameter(cmd, "MAIL_ADDR2", newCt.AddressLine2 );
                DatabaseWrapperHelper.AddInStringParameter(cmd, "MAIL_CITY", newCt.City );
                DatabaseWrapperHelper.AddInStringParameter(cmd, "MAIL_STATE", newCt.State );
                DatabaseWrapperHelper.AddInStringParameter(cmd, "MAIL_POSTAL_CODE", newCt.Zip );
                DatabaseWrapperHelper.AddInStringParameter(cmd, "MAIL_COUNTRY", newCt.Country );


            }, _context.Identity.InstitutionId);

            executeResult = _dbWrapper.Execute(cmd =>
            {
                cmd.CommandText = SqlConstants.CREATE_HOUSEHOLD_MEMBER;
                DatabaseWrapperHelper.AddInLongParameter(cmd, "HOUSEHOLD_ID", householdId.ToString());
                DatabaseWrapperHelper.AddInLongParameter(cmd, "INVESTOR_ID", investorId.ToString());
                DatabaseWrapperHelper.AddInIntParameter(cmd, "RELATIONSHIP_TYPE_ID", ((int)newCt.RelationShipType).ToString());
                DatabaseWrapperHelper.AddInStringParameter(cmd, "DEFAULT_ACCOUNT_ID", string.Empty);                
            }, _context.Identity.InstitutionId);
            newCt.InvestorId = investorId.ToString();
            if (executeResult > 0)
            {
                return true;
            }
            return false;
        }

        public void InsertAllATTR2IntoHH( string householdId)
        {
            DeleteAllATTR2FromHH( householdId);
             string insertSQL = @"insert into web_household_qry_filter(household_id, filter_token, user_entered_yn)(select wth.household_Id, wt.team_id, decode(wth.team_assign_type_id, 3, 'Y', 'N') from web_team_x_household wth, web_team wt where wth.team_id = wt.team_id and wt.team_hierarchy_type_id = 3 and wth.household_id = :householdId)";
            var executeResult = _dbWrapper.Execute(cmd =>
            {
                cmd.CommandText = insertSQL;
                DatabaseWrapperHelper.AddInIntParameter(cmd, "householdId", householdId.ToString());
            }, _context.Identity.InstitutionId);
        }

        private void DeleteAllATTR2FromHH( string householdId)
        {
            string DeleteSQL = @"DELETE FROM WEB_HOUSEHOLD_QRY_FILTER WHERE HOUSEHOLD_ID = :householdId";
            var executeResult = _dbWrapper.Execute(cmd =>
            {
                cmd.CommandText = DeleteSQL;
                DatabaseWrapperHelper.AddInIntParameter(cmd, "householdId", householdId.ToString());
             }, _context.Identity.InstitutionId);
            
        }

        public void UpdateRecentContact(string householdId, string investorid)
        {
            var executeResult = _dbWrapper.ExecuteScalar(cmd =>
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PKG_CRM.UPDATERECENTCONTACTS";
                DatabaseWrapperHelper.AddInIntParameter(cmd, "A_NBROKER_ID", _context.Identity.BrokerId);
                DatabaseWrapperHelper.AddInIntParameter(cmd, "A_NHOUSEHOLD_ID", householdId);
                DatabaseWrapperHelper.AddInIntParameter(cmd, "A_NINVESTOR_ID", investorid);
                DatabaseWrapperHelper.AddOutIntParameter(cmd, "A_UTSTATUS");
                DatabaseWrapperHelper.AddOutStringParameter(cmd, "A_UTSTATUSMSG", 2000);
            }, _context.Identity.InstitutionId);
           
        }

        //public TeamList GetHouseHoldTeams(int instId, long brokerId, string planId, string householdId)
        //{
        //    long _planId = -1;
        //    int _householdId = -1;
        //    TeamList teamList = null;
        //    string cacheKey = string.Format("{0}{1}{2}", planId, brokerId, householdId);
        //    if ((teamList = (TeamList)this.GetCachedObject(cacheKey)) != null) return teamList;
        //    teamList = new TeamList();
        //    //if household id is not empty then get it 
        //    if (householdId != string.Empty)
        //        _householdId = Int32.Parse(householdId);

        //    if (planId != string.Empty)
        //        _planId = Int32.Parse(planId);

        //    //if household id is null try to get from plan id
        //    if (householdId == "")
        //    {
        //        if (_planId > 0)
        //        {
        //            _householdId = (int)(ContactServiceDataGateway.GetPartyId(_context, _planId));
        //        }
        //    }

        //    AppConfigSvc _svc = new AppConfigSvc(this._context);
        //    bool _isBackOffice = _svc.IsBackOffice(instId);

        //    //get household teams only for backoffice institution
        //    if (_isBackOffice)
        //    {
        //        //if household id not present return as it means its new household
        //        if (_householdId != -1)
        //        {
        //            Thomson.Financial.Book.BusinessObjects.BrokerType brokerTYpe = ContactServiceDataGateway.GetBrokerTypeId(_context);

        //            TeamCollection teams = (new ho.HouseholdManager()).GetHouseholdTeams(this._context, brokerId, _householdId);

        //            if (teams != null)
        //            {
        //                foreach (bo.Team boTeam in teams)
        //                {
        //                    teamList.Add(boTeam);
        //                }
        //            }
        //        }

        //        if (teamList.Count == 0)
        //        {
        //            Thomson.Financial.Book.BusinessObjects.BrokerType brokerType = ContactServiceDataGateway.GetBrokerTypeId(_context);

        //            TeamCollection teams = (new Thomson.Financial.Service.BrokerService.BrokerManager()).GetBrokerTeamsForPreferences(this._context, brokerId, brokerType, 301);

        //            if (teams != null)
        //            {
        //                foreach (bo.Team boTeam in teams)
        //                {
        //                    teamList.Add(boTeam);
        //                }
        //            }

        //        }
        //    }
        //    this.AddCachedObject(cacheKey, teamList);
        //    return teamList;
        //}

    }
}
