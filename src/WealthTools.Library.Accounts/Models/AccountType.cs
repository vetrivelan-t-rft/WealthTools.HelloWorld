using System;
using System.Collections.Generic;
using System.Text;

namespace WealthTools.Library.Accounts.Models
{
    public class AccountType
    {

        public long AccountTypeId { get; set; } = 0;
        public long AccountTaxStatusId { get; set; } = 0;
        public string AccountTypeDesc { get; set; } = "";
        public string TaxTypeDesc { get; set; } = "";
    }
     
}
