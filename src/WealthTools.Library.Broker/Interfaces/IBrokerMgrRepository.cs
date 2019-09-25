using System;
using System.Collections.Generic;
//using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.BrokerManager.Models;

namespace WealthTools.Library.BrokerManager.Interfaces
{
    public interface IBrokerMgrRepository
    {
        Broker GetBrokerInfo();
        bool IsBackOfficeInstitution();

        List<AssetClass> GetAssetClassification(bool broadAssetClass);


    }
}
