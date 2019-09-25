using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace WealthTools.Library.Proposals.Config
{
    class Constants
    {
        public static readonly List<string> PlanTypeList = new List<string> { "14", "15" ,"17"};
        public static readonly List<string> ReportTypeList = new List<string>() { "73", "74", "75", "76", "77" };
        public const string PROPGEN_PROPOSAL_PATH = "578";
        public const string PROPOPOSAL_PATH_ENTITY_TYPE_ID = "2617";
        public const string HOLDING_LEVEL = "Account";       
    }

    enum planTypeList
    {
        oldProposal = 14,
        newProposal = 15,
        recentProposal = 17
    }
}
