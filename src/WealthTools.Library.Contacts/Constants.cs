using System;

namespace WealthTools.Library.Contacts
{
    public class Constants
    {
        public const bool IDS_IS_TOTAL_COUNT_ONLY = false;
        public const bool IDS_SUPP_ACCT = false;
        public const string IDS_START_ROW = "1";
        public const string IDS_COUNT = "50";
    }

    public enum RelationShipType
    {
        HEAD = 1,
        SPOUSE = 2,
        DEPENDENT = 3,
        THIRDPARTY = 4
    }
    public enum SearchBy
    {
        DEMOGRAPHICS = 1,
        ACCOUNTNUMBER = 2,
        REPCODE = 3
    }

    public enum HouseholdType
    {
        CLIENT = 1,
        PROSPECT = 2
    }
}
