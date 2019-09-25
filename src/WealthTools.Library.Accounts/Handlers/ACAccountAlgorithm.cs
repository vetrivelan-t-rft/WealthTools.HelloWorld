using System;
using System.Collections.Generic;
using System.Text;
using WealthTools.Library.Accounts.Models;
using WealthTools.Library.BrokerManager.Interfaces;
using WealthTools.Library.BrokerManager.Models;

namespace WealthTools.Library.Accounts.Handlers
{
    public class ACAccountAlgorithm
    {
        IBrokerMgrRepository _brokerMgrRepository;

        public ACAccountAlgorithm( IBrokerMgrRepository brokerMgrRepository)
        {
             _brokerMgrRepository = brokerMgrRepository;
        }
        private double ExtractSecurityAmount(List<OpenPosition> a_listOfPosition, ref SortedList<string, PositionAmountPair> a_mapOfSecAmount)
        {

            double a_dBalance = 0.0;
            if (a_listOfPosition != null)
            {
                foreach (OpenPosition openPos in a_listOfPosition)
                {
                    PositionAmountPair posAmount = new PositionAmountPair();
                    posAmount.m_lPositionID = openPos.PosId;
                    posAmount.m_dAmount = openPos.MarketValue;
                    a_dBalance += openPos.MarketValue;
                    if (a_mapOfSecAmount.ContainsKey(openPos.SecID) == false)
                    {
                        a_mapOfSecAmount.Add(openPos.SecID, posAmount);
                    }
                    else
                    {
                        PositionAmountPair posIncAmount = a_mapOfSecAmount[openPos.SecID];
                        posIncAmount.m_dAmount += posAmount.m_dAmount;
                    }
                }
            }
            return a_dBalance;
        }

        
        public AssetClassAccount Calculate(List<OpenPosition> listOfPosition)
        {
            AssetClassAccount acAcct = new AssetClassAccount();
            
           SortedList<string, PositionAmountPair> mapOfSecAmount = new SortedList<string, PositionAmountPair>();
            
            //acAcct.AccountId = accountId;

            double dBalance = ExtractSecurityAmount(listOfPosition, ref mapOfSecAmount);

            acAcct.Balance = dBalance;

            List<AssetClass> listOfAssetClass = _brokerMgrRepository.GetAssetClassification(false);
            //mapOfACSecurity = BuildACSecurityMap(listOfAssetClass);


            acAcct.ACPositions = new List<AssetClassPosition>();
            foreach (AssetClass assetObj in listOfAssetClass)
            {
                if (assetObj.AssetType == "O")
                    continue;

                string lGSecID = assetObj.SecId;
                long lPositionID = 0;
                double dPct = 0.0;
                mapOfSecAmount.TryGetValue(lGSecID, out PositionAmountPair posAmtPair);
                if (posAmtPair != null)
                {
                    lPositionID = mapOfSecAmount[lGSecID].m_lPositionID;
                    if (dBalance > 0.0)
                        dPct = ((mapOfSecAmount[lGSecID].m_dAmount) / dBalance) * 100;
                }
                AssetClassPosition acPos = new AssetClassPosition();
                acPos.assetClass = assetObj;
                acPos.Pct = dPct;
                acPos.assetClass.SecId = lGSecID;
                acPos.PositionId = lPositionID;
                acAcct.ACPositions.Add(acPos);                
            }
           return acAcct;
        }

    }
}
