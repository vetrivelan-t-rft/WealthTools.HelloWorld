using System;
using System.Collections.Generic;
using System.Text;
using WealthTools.Library.BrokerManager.Models;

namespace WealthTools.Library.Accounts.Models
{
   
    public class AssetClassAccount
    {
        public List<AssetClassPosition> ACPositions { get; set; } = new List<AssetClassPosition>();

        public double Balance {get;set;}
       public string AccountId { get; set; }
       
    }

    public class AssetClassPosition 
    {
        public AssetClass assetClass { get; set; } = new AssetClass();
        public double Pct { get; set; }

        public long PositionId { get; set; }
    }

    ////Create/Update Save Security Positions
    public class SaveAssetClassPositionsRequest
    {
        public string AccountId { get; set; }
        public double Balance { get; set; }
        public List<SaveAssetClassPosition> ACPositions { get; set; } = new List<SaveAssetClassPosition>();


    }
    public class SaveAssetClassPosition
    {
        public double Pct { get; set; }


        public SaveAssetClass assetClass { get; set; } = new SaveAssetClass();


    }
    public class SaveAssetClass
    {
        public string SecId { get; set; }
    }

    }


