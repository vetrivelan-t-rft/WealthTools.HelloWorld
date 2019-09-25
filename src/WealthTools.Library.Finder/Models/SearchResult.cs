using System.Collections.Generic;
using WealthTools.Library.Contacts.Models;
using WealthTools.Library.Proposals.Models;

namespace WealthTools.Library.Search.Models
{
   public class SearchResult
    {      
        public List<Household> HouseholdList { get; set; } = new List<Household>();
        public List<ProposalsModel> ProposalsList { get; set; } = new List<ProposalsModel>();

        public List<SecBasicData> secList = null;

    }

 
}