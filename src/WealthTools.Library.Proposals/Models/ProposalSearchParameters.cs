using System;
using System.Collections.Generic;
using System.Text;

namespace WealthTools.Library.Proposals.Models
{
    public enum SearchBy
    {
        DEMOGRAPHICS = 1,
        ACCOUNTNUMBER = 2,        
    }

    public class ProposalSearchParameters
    {
        public SearchBy Searchby { get; set; }
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
         public string AccountNumber { get; set; } = "";
       public long Count { get; set; } = 30;
       
       
    }
}
