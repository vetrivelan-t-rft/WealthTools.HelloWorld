using System;
using System.Collections.Generic;
using System.Text;
using WealthTools.Library.Contacts;

namespace WealthTools.Library.Contacts.Models
{
    public class SearchParameters
    {
        public SearchBy Searchby { get; set; }
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string City { get; set; } = "";
        public string State { get; set; } = "";
        public string Zip { get; set; } = "";
        public string AccountNumber { get; set; } = "";
        public string RepCode { get; set; } = "";

        //need to check
        public string StartRow { get;} = Constants.IDS_START_ROW;
        public string Count { get; } = Constants.IDS_COUNT;
        public string SortByColumn { get; set; } = "";
        public bool IsSortAsc { get; set; } = true;
       
    }
}
