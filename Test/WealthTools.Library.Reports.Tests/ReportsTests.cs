using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using WealthTools.Common.UnitTests.Utils;
using WealthTools.Library.Reports.Interfaces;
using WealthTools.Library.Reports.Models;
using WealthTools.Library.Reports.Repositories;
using Xunit;

namespace WealthTools.Library.Reports.Tests
{
    public class ReportsTests : UnitTestBase
    {
        
        protected IReportsRepository _repoRepository;
        public ReportsTests()
        {
            // Add the service you want to add like _serviceCollection.AddSingleton<IEncrypt, EncryptDecryptAES>();
            _serviceCollection.AddScoped<IReportsRepository, ReportsRepository>();            

            // Build the service
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            _repoRepository = (IReportsRepository)_serviceProvider.GetService(typeof(IReportsRepository));
                        
            _mockContext.Object.Identity.BrokerId = "13240178";
            _mockContext.Object.Identity.InstitutionId = "6083";
        }


        [Theory]
        [InlineData(4)]        
        public void GetReportListTest(int ProductId)
        {
            List<ReportInfo> res = _repoRepository.GetReportList(ProductId);
            Assert.True(res.Count > 0);
        }

        [Theory]        
        [InlineData(115)]
        public void GetTemplateReportListTest(int ProductModuleID)
        {
            List<TemplateReport> res = _repoRepository.GetTemplateReportList(ProductModuleID);
            Assert.True(res.Count > 0);
        }


    }
}
