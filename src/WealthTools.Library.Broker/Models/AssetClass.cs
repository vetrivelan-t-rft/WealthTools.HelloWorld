using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WealthTools.Library.BrokerManager.Models
{
    public class CMAData 
    {
        public double Risk { get; set; } = 0;

        public double Yield { get; set; } = 0;

        public double Return { get; set; } = 0;
        public bool IsYieldtaxable { get; set; } = false;
        public string AssetClassName { get; set; }

        public long CMAAssetClassId { get; set; }

       
    }
    public class AssetClass
    {

        public string Abbreviation { get; set; }
        public long Acid { get; set; }
        public long BroadACId { get; set; }
        public string AssetType { get; set; }
        public long Blue { get; set; }
        public string Descr { get; set; }
        public long Green { get; set; }
        public long Idxid { get; set; }
        public string IndexName { get; set; }
        public string Name { get; set; }
        public string PerfDescr { get; set; }
        public long Red { get; set; }
        public long SortOrder { get; set; }
        public string SecId { get; set; }
      

    }
}
