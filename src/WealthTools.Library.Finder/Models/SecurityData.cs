using System;
using System.Collections.Generic;
using System.Text;

namespace WealthTools.Library.Search.Models
{
    class SearchSecurityData
    {
        public List<SecBasicData> secList = new List<SecBasicData>();
    }

    public class SecBasicData 
    {
       
        
        public string SubType{get;set;}
        
        public string PriceFactor { get; set; }

        public string SecID { get; set; }

        public string CogID { get; set; }

        public string Price { get; set; }


        public string MFID { get; set; }

        public string Name { get; set; }


        public string Ticker { get; set; }


        public string CusipNum { get; set; }


        public string SecNo { get; set; }


        public string SecType { get; set; }


        public bool DisplayFactSheetLink { get; set; }
    }

}
