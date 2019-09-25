using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using WealthTools.Common.UnitTests.Utils;
using WealthTools.Library.BrokerManager.Interfaces;
using WealthTools.Library.BrokerManager.Models;
using WealthTools.Library.BrokerManager.Repositories;
using Xunit;

namespace WealthTools.Library.BrokerManager.Tests
{
    public class BrokerManagerTest : UnitTestBase
    {
      

        protected IBrokerMgrRepository _brokerMgrRepository;
        public BrokerManagerTest()
        {
            // Add the service you want to add like _serviceCollection.AddSingleton<IEncrypt, EncryptDecryptAES>();
            _serviceCollection.AddScoped<IBrokerMgrRepository, BrokerMgrRepository>();

            // Build the service
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            _brokerMgrRepository = (IBrokerMgrRepository)_serviceProvider.GetService(typeof(IBrokerMgrRepository));

            // Modify the mock object in case needed like  _mockContext.Object.Identity.UserName = "PRatip";
            _mockContext.Object.Identity.BrokerId = "11047987";
            _mockContext.Object.Identity.InstitutionId = "6027";
        }

        [Fact]
        public void GetBrokerInfoTest()
        {
            Broker broker = _brokerMgrRepository.GetBrokerInfo();

            Assert.NotNull(broker);
        }

        [Fact]
        public void IsBackOfficeTest()
        {
            bool result = _brokerMgrRepository.IsBackOfficeInstitution();

            Assert.True(result);
        }

        [Fact]
        public void GetAssetClassification()
        {
            List<AssetClass> result = _brokerMgrRepository.GetAssetClassification(false);

            Assert.True(result.Count > 0);
        }

        [Fact]
        public void GetAssetClassificationBroadAssetClass()
        {
            List<AssetClass> result = _brokerMgrRepository.GetAssetClassification(false);

            Assert.True(result.Count > 0);
        }
    }
}
