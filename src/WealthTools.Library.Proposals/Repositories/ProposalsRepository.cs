using System;
using System.Collections.Generic;
using System.Data;
using WealthTools.Common.DatabaseConnection;
using WealthTools.Common.DatabaseWrapper;
using WealthTools.Common.DatabaseWrapper.Interfaces;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.Proposals.Interfaces;
using WealthTools.Library.Proposals.Models;
using WealthTools.Library.Proposals.Config;
using System.Linq;
using System.Collections;

namespace WealthTools.Library.Proposals.Repositories
{
    public class ProposalsRepository : IProposalsRepository
    {
        readonly IDatabaseWrapper _dbWrapper;
        readonly IContext _context;        

        public ProposalsRepository(IDatabaseWrapper dbWrapper, IContext context)
        {
            _dbWrapper = dbWrapper;
            _context = context;            
        }
        public List<ProposalsModel> GetRecentProposals()
        {
            string planTypes = string.Join(",", Enum.GetValues(typeof(planTypeList)).Cast<int>());
            List<ProposalsModel> proposalList = new List<ProposalsModel>();
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd => {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PKG_PG.GETRECENTPROPOSALS";
                DatabaseWrapperHelper.AddInIntParameter(cmd, "A_NBROKER_ID", _context.Identity.BrokerId);
                DatabaseWrapperHelper.AddInStringParameter(cmd, "A_VPLANTYPELIST", planTypes);
                DatabaseWrapperHelper.AddOutCursorParameter(cmd, "A_RCPROPOSALLIST");
                DatabaseWrapperHelper.AddOutIntParameter(cmd, "A_UTSTATUS");
                DatabaseWrapperHelper.AddOutStringParameter(cmd, "A_UTSTATUSMSG", 2000);

            }, _context.Identity.InstitutionId);
            foreach (var row in records)
            {
                ProposalsModel proposal = new ProposalsModel();
                proposal.ProposalId = row["PLAN_ID"].ToString();
                proposal.ProposalName = (!string.IsNullOrEmpty(row["PLAN_NAME"].ToString())) ? row["PLAN_NAME"].ToString(): string.Empty;
                proposal.PartyID = row["PARTY_ID"].ToString();
                proposal.PartyName = row["PARTY_NAME"].ToString();
                proposal.PartyType = row["PARTY_TYPE_NAME"].ToString();
                proposal.LastModifiedDate = row["ACCESS_DATE"].ToString();
                proposal.IsEntitle = row["FULL_ENT"].ToString();
                proposal.RowNum = Int32.Parse(row["RN"].ToString());
                proposal.PlanTypeId = row["PLAN_TYPE_ID"].ToString();
                proposal.ProgramName = row["PROGRAM_NAME"].ToString();
                proposal.IsNoPartialEntitlement = row["NO_ENT"].ToString();
                proposal.ModelMinorVersion = row["MODEL_MINOR_VERSION"].ToString();

                proposalList.Add(proposal);
            }
            return proposalList;
        }

        public List<ProposalByHH> GetProposalsByHH(string householdID)
        {
            List<string> planTypeList = Constants.PlanTypeList;
            List<string> reportTypeList = Constants.ReportTypeList;
            List<ProposalByHH> proposalList = new List<ProposalByHH>();

            string sql = SqlConstants.GET_PROPOSALS_BY_HH;
            sql = _dbWrapper.BuildSqlInClauseQuery(planTypeList, ":PLAN_TYPE_IDS", sql);
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                _dbWrapper.BuildParamInClauseQuery(planTypeList, "PLAN_TYPE_IDS", cmd);
                DatabaseWrapperHelper.AddInIntParameter(cmd, ":HOUSEHOLD_ID", householdID);
                DatabaseWrapperHelper.AddInIntParameter(cmd, ":PARTY_TYPE_ID", "1");
            }, _context.Identity.InstitutionId);
            foreach (var row in records)
            {
                ProposalByHH proposal = new ProposalByHH();
                proposal.ProposalId = row["PLAN_ID"].ToString();
                proposal.ProposalName = (!string.IsNullOrEmpty(row["NAME"].ToString())) ? row["NAME"].ToString() : string.Empty;
                proposal.PlanTypeId = row["PLAN_TYPE_ID"].ToString();
                proposal.ProgramName = (!string.IsNullOrEmpty(row["PROGRAM_NAME"].ToString())) ? row["PROGRAM_NAME"].ToString() : string.Empty;
                proposal.ModelMinorVersion = row["MODEL_MINOR_VERSION"].ToString();
                proposal.LastModifiedDate = row["LAST_MODIFIED_DATE"].ToString();

                proposalList.Add(proposal);
            }

            foreach (var plan in proposalList)
            {
                List<string> planIds = new List<string>() { plan.ProposalId };
                var sqlReports = SqlConstants.GET_INV_REPORTS_BY_HH;
                sqlReports = _dbWrapper.BuildSqlInClauseQuery(planIds, ":PLAN_IDS", sqlReports);
                sqlReports = _dbWrapper.BuildSqlInClauseQuery(reportTypeList, ":REPORT_TYPE_IDS", sqlReports);
                sqlReports = _dbWrapper.BuildSqlInClauseQuery(planTypeList, ":PLAN_TYPE_IDS", sqlReports);
                IEnumerable <IDataRecord> records1 = _dbWrapper.QueryDataRecord(cmd =>
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sqlReports;
                    DatabaseWrapperHelper.AddInIntParameter(cmd, ":BROKER_ID", _context.Identity.BrokerId);
                    _dbWrapper.BuildParamInClauseQuery(planIds, "PLAN_IDS", cmd);
                    _dbWrapper.BuildParamInClauseQuery(reportTypeList, "REPORT_TYPE_IDS", cmd);
                    _dbWrapper.BuildParamInClauseQuery(planTypeList, "PLAN_TYPE_IDS", cmd);
                }, _context.Identity.InstitutionId);

                foreach(var reportRow in records1)
                {
                    ArchivedReportInfo reportInfo = new ArchivedReportInfo();
                    reportInfo.Name = reportRow["NAME"].ToString();
                    reportInfo.ReportId = reportRow["ARCHIVED_REPORT_ID"].ToString();
                    reportInfo.PlanId = reportRow["PLAN_ID"].ToString();
                    reportInfo.FileName = reportRow["FILENAME"].ToString();
                    reportInfo.Path = reportRow["LOCATION"].ToString();
                    reportInfo.EntitledYN = reportRow["ENTITLED_YN"].ToString();
                    reportInfo.IS_STREETSCAPE = reportRow["STREETSCAPEYN"].ToString();
                    plan.ReportInfoList.Add(reportInfo);
                }

            }

            return proposalList;
        }

        public bool Delete_Web_Investment_Plan(int Planid)
        {

            var executeResult = _dbWrapper.Execute(cmd =>
            {
                cmd.CommandText = SqlConstants.DELETE_WEB_INVESTMENT_PLAN;
                DatabaseWrapperHelper.AddInIntParameter(cmd, ":PLAN_ID", Planid.ToString());                         
            }, _context.Identity.InstitutionId);

            if (executeResult > 0)
            {
                return true;
            }
            return false;
        }

        public long CreateNewProposal( string houseHoldID, string proposalName )
        {
            
            long planId = -1;
            long solutionID = -1;
            string portfolioID = "-1";
            int defaultPropAClassLevelId = -1;
            long processID = -1;
            long processLogID = -1;
            long investorID = -1;

            defaultPropAClassLevelId = Convert.ToInt32( _dbWrapper.ExecuteScalar(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_DEFAULT_ASSETCLASS_VIEW_ID; ;
                DatabaseWrapperHelper.AddInLongParameter(cmd, ":GROUP_ID", _context.Identity.GroupId);
            }, _context.Identity.InstitutionId));

            solutionID = Convert.ToInt32(_dbWrapper.ExecuteScalar(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_SEQUENCE_SOLUTION;                 
            }, _context.Identity.InstitutionId));     

            
            //Create PlanID
            planId = Convert.ToInt32(_dbWrapper.ExecuteScalar(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_SEQUENCE_INVESTMENT_PLAN;
            }, _context.Identity.InstitutionId));

            //Create PlanID
            processID = Convert.ToInt32(_dbWrapper.ExecuteScalar(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_SEQUENCE_PROCESS;
            }, _context.Identity.InstitutionId));
            processLogID = Convert.ToInt32(_dbWrapper.ExecuteScalar(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_SEQUENCE_PROCESS_LOG;
            }, _context.Identity.InstitutionId));

            portfolioID = GetSequencePattern("WEB_PORTFOLIO_PRODUCT", "");
            //Get the default profile and Model Ids
            SortedList<int, string> _list = GetDefaultProfileAndModelId();
            IEnumerator<KeyValuePair<int, string>> itVal = _list.GetEnumerator();
            int profileid = -1;
            string modelId = string.Empty;
            string defaultProgramId = string.Empty; ;
            string defaultProposalPath = string.Empty;
            while (itVal.MoveNext())
            {
                profileid = Int32.Parse(itVal.Current.Key.ToString());
                modelId = itVal.Current.Value.ToString();                
            }
            //Need to revisit 
            //ProposalPathCollection proposalPaths = new ProposalPathCollection();
            //proposalPaths = GetProposalList(context);
            //string defaultProposalPath = (from proposalPath in proposalPaths.List
            //                              where proposalPath.IsDefault == true
            //                              select proposalPath.ProposalPathId).FirstOrDefault();
            //while (itVal.MoveNext())
            //{
            //    profileid = Int32.Parse(itVal.Key.ToString());
            //    modelId = itVal.Value.ToString();
            //    if (!string.IsNullOrEmpty(modelId) && defaultProposalPath.ToUpper() != ProposalPathType.NO.ToString() && defaultProposalPath.ToUpper() != ProposalPathType.YES.ToString())
            //        defaultProgramId = GetPortfolioProgramId(modelId, context);
            //}
            List<Action<IDbCommand>> configureCommandList = new List<Action<IDbCommand>>();
            if (solutionID != -1)
            {

                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.CREATE_SOLUTION;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "SOLUTION_ID", solutionID.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "NAME", "");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "DESCR", "");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "CREATED_BY", _context.Identity.BrokerId);
                }));
                //Create WEB_SOLUTION_X_PARTY
                int.TryParse(houseHoldID, out int householdID);
                if (householdID > 0)
                {
                    //Get InvestorID
                    investorID = Convert.ToInt32(_dbWrapper.ExecuteScalar(cmd =>
                    {
                        cmd.CommandText = SqlConstants.GET_INVESTOR_ID;
                        DatabaseWrapperHelper.AddInIntParameter(cmd, ":HOUSEHOLD_ID", householdID.ToString());
                    }, _context.Identity.InstitutionId));

                    configureCommandList.Add(new Action<IDbCommand>(cmd =>
                    {
                        cmd.CommandText = SqlConstants.CREATE_SOLUTION_PARTY;
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "SOLUTION_ID", solutionID.ToString());
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "PARTY_ID", householdID.ToString());
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "PARTY_TYPE_ID", "1");
                    }));

                    configureCommandList.Add(new Action<IDbCommand>(cmd =>
                    {
                        cmd.CommandText = SqlConstants.CREATE_SOLUTION_PARTY;
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "SOLUTION_ID", solutionID.ToString());
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "PARTY_ID", investorID.ToString());
                        DatabaseWrapperHelper.AddInStringParameter(cmd, "PARTY_TYPE_ID", "2");
                    }));
                }
            }
            if (planId != -1)
            {
                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.CREATE_INVESTMENT_PORTFOLIO;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PORTFOLIO_ID", portfolioID);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PORTFOLIO_TYPE_ID", "0");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "CREATE_BY_USER_ID", _context.Identity.BrokerId);
                }));

                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.CREATE_HOLDING_PORTFOLIO;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PORTFOLIO_ID", portfolioID);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PORTFOLIO_TYPE_ID", "0");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "EXTERNAL_ID", "");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "DATASOURCE_ID", "1");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "CREATE_BY_USER_ID", _context.Identity.BrokerId);
                }));

                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.CREATE_WEB_PROCESS;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PROCESS_ID", processID.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PROCESS_DEFINITION_ID", "1");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "BROKER_ID", _context.Identity.BrokerId);
                }));

                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.CREATE_WEB_PROCESS_PARAM;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PROCESS_ID", processID.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "SEQ_NUM", "1");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ENTITY_TYPE_ID", Constants.PROPOPOSAL_PATH_ENTITY_TYPE_ID);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ENTITY_ID", defaultProposalPath);
                }));
                //create web_process_activity
                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.CREATE_WEB_PROCESS_ACTIVITY;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PROCESS_ID", processID.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "BROKER_ID", _context.Identity.BrokerId);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ACTIVITY_ID", "1");
                }));

                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.CREATE_WEB_PROCESS_ACTIVITY_LOG;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PROCESS_ID", processID.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ACTIVITY_ID", "1");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "BROKER_ID", _context.Identity.BrokerId);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "ENTITY_TYPE_ID", "779");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "LOG_ENTRY_ID", processLogID.ToString());
                }));
                //Create web_investment_plan
                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.CREATE_NEW_PROPOSAL;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PLAN_ID", planId.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "SOLUTION_ID", solutionID.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "NAME", proposalName);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "DESCR", "");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PROFILE_ID", profileid.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "MODEL_ID", modelId);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PROGRAM_ID", defaultProgramId);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "HOLDING_PORTFOLIO_ID", portfolioID);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PLAN_PROCESS_ID", processID.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "CASH_AMOUNT", "0");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "MARK_DELETED_YN", "N");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "CREATED_BY", _context.Identity.BrokerId);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "AC_VIEW_ID", defaultPropAClassLevelId.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "MODELBASED_YN", !string.IsNullOrEmpty(modelId) ? "Y" : "N");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "HOLDINGLEVEL", "Account");
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PLAN_TYPE_ID", "17");
                }));
                //insert the data in the web_broker_recent_plan and delete the old ones if the count is more than 30
                configureCommandList.Add(new Action<IDbCommand>(cmd =>
                {
                    cmd.CommandText = SqlConstants.INSERT_RECENT_PROPOSAL;
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "BROKER_ID", _context.Identity.BrokerId);
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PLAN_ID", planId.ToString());
                    DatabaseWrapperHelper.AddInStringParameter(cmd, "PLAN_TYPE_ID", Convert.ToInt32(planTypeList.recentProposal).ToString());                   
                }));
            }
            bool result = _dbWrapper.ExecuteBatch(configureCommandList.ToArray(), _context.Identity.InstitutionId);
            return planId;
        }

        private string GetSequencePattern(string tableName, string recordType)
        {
            string seq = _dbWrapper.ExecuteScalar(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_SEQUENCE_PORTFOLIO;
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":Pi_Inst_ID", _context.Identity.InstitutionId.ToString());
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":Pi_Table_Name", tableName);
                DatabaseWrapperHelper.AddInStringParameter(cmd, ":Pi_Record_Type", recordType);
            }, _context.Identity.InstitutionId).ToString();            
            return seq;
        }

        private  SortedList<int, string> GetDefaultProfileAndModelId()
        {
            SortedList<int, string> defaultProfileAndModelId = new SortedList<int, string>();
            List<string> defaultOptonIds = new List<string>();
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_DEFAULT_OPTIONS_IDS;
                DatabaseWrapperHelper.AddInIntParameter(cmd, ":a_nGroupId", _context.Identity.GroupId);
            }, _context.Identity.InstitutionId);
            foreach (var row in records)
            {
                defaultOptonIds.Add(row["INV_PROFILE_FACTOR_OPTION_ID"].ToString());
            }

            var sql = SqlConstants.GET_DEFAULT_PROFILE_MODEL_ID;
            sql = _dbWrapper.BuildSqlInClauseQuery(defaultOptonIds, ":OPTION_IDS", sql);
             records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                _dbWrapper.BuildParamInClauseQuery(defaultOptonIds, "OPTION_IDS", cmd);
                DatabaseWrapperHelper.AddInIntParameter(cmd, ":GROUP_ID", _context.Identity.GroupId);
                DatabaseWrapperHelper.AddInIntParameter(cmd, ":OPTION_COUNT", defaultOptonIds.Count.ToString());

            }, _context.Identity.InstitutionId);

            foreach (var row in records)
            {
                int _Id = Int32.Parse(row["PROFILE_DTL_ID"].ToString());
                string _modelId = row["MODEL_ID"].ToString();
                defaultProfileAndModelId[_Id] = _modelId;
            }
            return defaultProfileAndModelId;        
        }

    }

}
