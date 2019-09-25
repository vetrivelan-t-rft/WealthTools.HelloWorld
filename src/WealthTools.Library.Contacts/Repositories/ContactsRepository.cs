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

        public List<Household> SearchAllContacts(SearchParameters searchParameters)
        {
            List<Household> resultList = new List<Household>();
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
                Household result = new Household();
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

        public Household CreateProspectHousehold(List<Contact> contactsListColl)
        {
            Household res =  CreateHousehold(contactsListColl);
            List<Team> teams = GetHouseHoldTeams(res.HouseholdID);
            UpdateTeamsForHousehold(res.HouseholdID, teams);
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
            DatabaseWrapperHelper.AddInIntParameter(cmd, "A_NCOUNT", searchParameters.Count.ToString());
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_NSORTBYCOL", "");
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VASC_YN", (searchParameters.IsSortAsc) ? "Y" : "N");
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VTOTCNTONLY_YN",  "N");
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_SUPPACCT",  "Y");
        }

        private void SetDemographicParameters(IDbCommand cmd, SearchParameters searchParameters)
        {
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VLNAME", HandleSpecialCharacters(searchParameters.LastName));
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VFNAME", HandleSpecialCharacters(searchParameters.FirstName));
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VSSN_TIN", string.Empty);
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VCITY", HandleSpecialCharacters(searchParameters.City));
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VSTATE", HandleSpecialCharacters(searchParameters.State));
            DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VZIP", HandleSpecialCharacters(searchParameters.Zip));
            DatabaseWrapperHelper.AddInStringParameter(cmd, "a_HouseholdLevel", "Y");

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
        private Household CreateHousehold(List<Contact> contacts)
        {
            Household result = new Household();
            Dictionary<long, long> contactDic = new Dictionary<long, long>();
            string hhName = "Client Household";
            //Get next householdid
            long householdId = Convert.ToInt64(_dbWrapper.ExecuteScalar(cmd =>
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
            var executeResult = _dbWrapper.Execute(cmd =>
            {
                cmd.CommandText = SqlConstants.CREATE_WEB_HOUSEHOLD_QRY_FILTER;
                DatabaseWrapperHelper.AddInIntParameter(cmd, "householdId", householdId.ToString());
            }, _context.Identity.InstitutionId);
        }

        private void DeleteAllATTR2FromHH( string householdId)
        {
            var executeResult = _dbWrapper.Execute(cmd =>
            {
                cmd.CommandText = SqlConstants.DELETE_WEB_HOUSEHOLD_QRY_FILTER;
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

        public bool IsBackOfficeInstitution()
        {
            int configId = Convert.ToInt32(_dbWrapper.ExecuteScalar(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_INSTITUTION_CONFIG;
                DatabaseWrapperHelper.AddInIntParameter(cmd, "InstitutionId",  _context.Identity.InstitutionId);                
            }, _context.Identity.InstitutionId));
            return configId == Constants.IDS_BACKOFFICE_CONFIG_ID;
        }

        public List<Team> GetHouseHoldTeams(string householdId)
        {
            List<Team> teamList = new List<Team>();
            //get household teams only for backoffice institution
            if (IsBackOfficeInstitution())
            {
                IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
                {
                    cmd.CommandText = SqlConstants.GET_HOUSEHOLD_TEAMS;
                    DatabaseWrapperHelper.AddInLongParameter(cmd, "hierarchyType", "3");
                    DatabaseWrapperHelper.AddInLongParameter(cmd, "household_id", householdId);
                }, _context.Identity.InstitutionId);
                foreach (var row in records)
                {
                    Team team = new Team();
                    team.TeamId = row["TEAM_ID"].ToString();
                    team.RepCode = row["REP_CODE"].ToString();
                    team.Name = row["NAME"].ToString();
                    team.Role = (Team.TeamRole)(Convert.ToInt64(row["TEAM_ROLE_ID"]));
                    team.AssignType = (Team.TeamAssignmentType)(Convert.ToInt64(row["TEAM_ASSIGN_TYPE_ID"]));
                    team.HierarchyType = (Team.TeamHierarchyType)(Convert.ToInt64(row["TEAM_HIERARCHY_TYPE_ID"]));
                    teamList.Add(team);
                }
            }
            if (teamList.Count == 0)
            {
                IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
                {
                    cmd.CommandText = SqlConstants.GET_DEFAULT_TEAMS;
                    DatabaseWrapperHelper.AddInLongParameter(cmd, "brokerId", _context.Identity.BrokerId);
                   
                }, _context.Identity.InstitutionId);
                foreach (var row in records)
                {
                    Team team = new Team();
                    team.Broker_ID = row["BROKER_ID"].ToString();
                    team.TeamId = row["TEAM_ID"].ToString();
                    team.RepCode = row["REP_CODE"].ToString();
                    team.Name = row["NAME"].ToString();
                    //team.City = row["CITY"].ToString();
                    //team.State = row["STATE"].ToString();
                    team.Role = (Team.TeamRole)(Convert.ToInt64(row["TEAM_ROLE_ID"]));
                    team.AssignType = Team.TeamAssignmentType.UserCreated;
                    team.HierarchyType = (Team.TeamHierarchyType)(Convert.ToInt64(row["TEAM_HIERARCHY_TYPE_ID"]));
                    teamList.Add(team);
                }
            }
                return teamList;
        }
        public void UpdateTeamsForHousehold(string householdId, List<Team> listOfTeam)
        {
            if (listOfTeam != null && listOfTeam.Count > 0)
            {
                List<Team> finalHHTeams = CalculateListOfTeams(householdId, listOfTeam);

                List<Action<IDbCommand>> configureCommandList = new List<Action<IDbCommand>>();
                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.DELETE_TeamEntitlement;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "householdId", householdId);
                }));

                bool containsPrimary = false;
                foreach (Team team in listOfTeam)
                {
                    if (team.Role == Team.TeamRole.Primary && (int)team.AssignType != 0)
                    {
                        if (!containsPrimary)
                            containsPrimary = true;
                        else
                            team.Role = Team.TeamRole.Secondary;
                    }
                    configureCommandList.Add(new Action<IDbCommand>(cmd =>
                    {
                        cmd.CommandText = team.AssignType != Team.TeamAssignmentType.UnKnown ? SqlConstants.CREATE_TEAM_ENTITLEMENT : SqlConstants.CREATE_TEAM_ENTITLEMENT_ASSIGNTYPE_0;
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "teamId", team.TeamId);
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "householdId", householdId);
                        if (team.AssignType != Team.TeamAssignmentType.UnKnown)
                        {
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "teamRole", ((int)team.Role).ToString());
                            DatabaseWrapperHelper.AddInStringParameter(cmd, "teamAssignmentType", ((int)team.AssignType).ToString());
                        }
                    }));
                }
                bool result = _dbWrapper.ExecuteBatch(configureCommandList.ToArray(), _context.Identity.InstitutionId);

            }
        }

        private  List<Team> CalculateListOfTeams(string householdId, List<Team> listOfTeams)
        {
            List<Team> lstExistingTeams = new List<Team>(), lstFinalTeams = new List<Team>();

            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_HOUSEHOLD_TeamEntitlement;
                DatabaseWrapperHelper.AddInLongParameter(cmd, "householdId", householdId);

            }, _context.Identity.InstitutionId);
            foreach (var row in records)
            {
                Team team = new Team();
                team.TeamId = row["TEAM_ID"].ToString();
                team.Role = (Team.TeamRole)(Convert.ToInt64(row["TEAM_ROLE_ID"]));
                team.AssignType = (Team.TeamAssignmentType)(Convert.ToInt64(row["TEAM_ASSIGN_TYPE_ID"]));
                lstExistingTeams.Add(team);
            }

            List<Team> tc = GetParentTeams( listOfTeams);
            if (tc != null)
            {
                foreach (Team t in tc)
                {
                    listOfTeams.Add(t);
                }
            }

            if (lstExistingTeams == null) return listOfTeams;

            if (listOfTeams != null)
            {
                //need to take care of user created team removed from UI case.
                foreach (Team oldTeam in lstExistingTeams)
                {
                    bool isExist = false;
                    foreach (Team newTeam in listOfTeams)
                    {
                        if (newTeam.TeamId == oldTeam.TeamId)
                        {
                            isExist = true;
                            lstFinalTeams.Add(newTeam);
                            break;
                        }
                    }
                }
                foreach (Team newTm in listOfTeams)
                {
                    bool isExist = false;
                    foreach (Team oldTm in lstExistingTeams)
                    {
                        if (oldTm.TeamId == newTm.TeamId)
                        {
                            isExist = true;
                            break;
                        }
                    }
                    if (!isExist)
                    {
                        lstFinalTeams.Add(newTm);
                    }
                }
            }

                    
            return lstFinalTeams;
        }

        private  List<Team> GetParentTeams(List<Team> listOfTeam)
        {
            List<Team> parentTeams = new List<Team>();
            if (listOfTeam.Count == 0) return null;
            List<string> teamIDList = new List<string>();
            foreach(Team team in listOfTeam)
            {
                teamIDList.Add(team.TeamId);
            }
            string sql = SqlConstants.GET_Parent_Teams;
            sql = _dbWrapper.BuildSqlInClauseQuery(teamIDList, ":TEAM_IDS", sql);
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                _dbWrapper.BuildParamInClauseQuery(teamIDList, "TEAM_IDS", cmd);                
            }, _context.Identity.InstitutionId);
            foreach (var row in records)
            {
                Team team = new Team();
                team.TeamId = row["TEAM_ID"].ToString();
                team.RepCode = row["REP_CODE"].ToString();
                team.Name = row["NAME"].ToString();
                team.Role = (Team.TeamRole)(Convert.ToInt64(row["TEAM_ROLE_ID"]));
                team.AssignType = (Team.TeamAssignmentType)(Convert.ToInt64(row["TEAM_ASSIGN_TYPE_ID"]));
                team.HierarchyType = (Team.TeamHierarchyType)(Convert.ToInt64(row["TEAM_HIERARCHY_TYPE_ID"]));
                parentTeams.Add(team);
            }
            return parentTeams;
        }

        public bool UpdateContact(Contact contact)
        {
            int executeResult = _dbWrapper.Execute(cmd =>
            {
                cmd.CommandText = SqlConstants.UPDATE_CONTACT;
                DatabaseWrapperHelper.AddInLongParameter(cmd, "INVESTOR_ID", contact.InvestorId);           
                DatabaseWrapperHelper.AddInStringParameter(cmd, "FIRST_NAME", contact.FirstName);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "MID_INITIAL", contact.MiddleName);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "LAST_NAME", contact.LastName);               
                //Home and Mail correspondence
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":EMAIL_ADDR", contact.PersonalEmail);
                //Home and Mail Address
                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOME_ADDR1", contact.AddressLine1);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOME_ADDR2", contact.AddressLine2);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOME_CITY", contact.City);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOME_STATE", contact.State);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "HOME_POSTAL_CODE", contact.Zip);
            }, _context.Identity.InstitutionId);

            
            if (executeResult > 0)
            {
                return true;
            }
            return false;
        }
    }
}
