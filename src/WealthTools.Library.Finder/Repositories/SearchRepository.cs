using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WealthTools.Common.DatabaseConnection;
using WealthTools.Common.DatabaseWrapper;
using WealthTools.Common.DatabaseWrapper.Interfaces;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.Contacts;
using WealthTools.Library.Contacts.Interfaces;
using WealthTools.Library.Search.Interfaces;
using WealthTools.Library.Search.Models;
using WealthTools.Library.Proposals.Interfaces;

namespace WealthTools.Library.Search.Repositories
{
    public class SearchRepository : ISearchRepository
    {
        readonly IDatabaseWrapper _dbWrapper;
        readonly IContext _context;
        readonly IContactsRepository _contactsRepository;
        readonly IProposalsRepository _proposalsRepository;

        public SearchRepository(IDatabaseWrapper dbWrapper, IContext context, IContactsRepository contactsRepository, IProposalsRepository proposalsRepository)
        {
            _dbWrapper = dbWrapper;
            _context = context;
            _contactsRepository = contactsRepository;
            _proposalsRepository = proposalsRepository;
        }        

        public SearchResult Search(AdvanceSearchParams searchParameters)
        {
            SearchResult result = new SearchResult();
            WealthTools.Library.Contacts.Models.SearchParameters param = new WealthTools.Library.Contacts.Models.SearchParameters()
            {
                Searchby = searchParameters.Searchby,
                LastName = searchParameters.LastName,
                FirstName = searchParameters.FirstName,
                AccountNumber = searchParameters.AccountNumber,
                Count = searchParameters.Count
            };
            result.HouseholdList= _contactsRepository.SearchAllContacts(param);

            //proposal Search
            WealthTools.Library.Proposals.Models.ProposalSearchParameters propsoalparam = new WealthTools.Library.Proposals.Models.ProposalSearchParameters()
            {
                Searchby = (WealthTools.Library.Proposals.Models.SearchBy)searchParameters.Searchby,
                LastName = searchParameters.LastName,
                FirstName = searchParameters.FirstName,
                AccountNumber = searchParameters.AccountNumber,
                Count = searchParameters.Count
            };
            result.ProposalsList = _proposalsRepository.SearchProposals(propsoalparam);

            
            return result;
        }

        public SearchResult SearchSecurities( string searchString)
        {
            SearchResult retList = new SearchResult { HouseholdList = null,ProposalsList=null,secList= new List<SecBasicData>() };
            List<string> searchStrings = searchString.ToUpper().Split(',').ToList<string>();

            string sql = SqlConstants.SearchSecurities;
            sql = _dbWrapper.BuildSqlInClauseQuery(searchStrings, ":REPLACE_SEARCH_KEYS", sql);
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                _dbWrapper.BuildParamInClauseQuery(searchStrings, "REPLACE_SEARCH_KEYS", cmd);               
            }, _context.Identity.InstitutionId);

            foreach (var row in records)
            {
                SecBasicData secDataInfo = new SecBasicData();
                secDataInfo.SecID = row["SEC_ID"].ToString(); ;
                secDataInfo.SecType = row["SECURITY_TYPE"].ToString();
                secDataInfo.Name = row["SECURITY_NAME"].ToString(); 
                secDataInfo.CusipNum = row["CUSIP"].ToString(); 
                secDataInfo.MFID = row["TA_ID"].ToString(); 
                secDataInfo.Ticker = row["SYMBOL"].ToString(); 
                secDataInfo.SecNo = row["SEC_NO"].ToString(); 
                secDataInfo.CogID = row["COG_ID"].ToString();
                //if (useSpecialFactsheetHandling && secDataInfo.SecType == "MA")
                //{
                //    if (!string.IsNullOrEmpty(row["PERSON_ID"].ToString(); FACTSHEET_FILE_NAME)))
                //    {
                //        secDataInfo.DisplayFactSheetLink = true;
                //    }
                //}
                //else
                //    if (!string.IsNullOrEmpty(secDataInfo.CogID)) secDataInfo.DisplayFactSheetLink = true;
                secDataInfo.Price = row["LAST_CLOSE_PRICE"].ToString(); 
                secDataInfo.PriceFactor = row["PRICE_FACTOR"].ToString(); 
                secDataInfo.SubType = row["SUB_TYPE"].ToString(); 
                retList.secList.Add(secDataInfo);                
            }               

            return retList;
        }

    }
}
