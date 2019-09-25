using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using WealthTools.Common.Utils.WebAPI;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using WealthTools.Library.Proposals.Interfaces;
using WealthTools.Library.Proposals.Repositories;
using WealthTools.Common.Logger.Interfaces;
using WealthTools.Library.Accounts.Repositories;
using WealthTools.Library.Accounts.Interfaces;
using WealthTools.Library.Reports;
using WealthTools.Library.BrokerManager.Interfaces;
using WealthTools.Library.BrokerManager.Repositories;
using WealthTools.Common.ReportEngine;
using WealthTools.Library.Contacts.Repositories;
using WealthTools.Library.Contacts.Interfaces;
using WealthTools.Library.Search.Interfaces;
using WealthTools.Library.Search.Repositories;
using WealthTools.Library.Reports.Interfaces;
using WealthTools.Library.Reports.Repositories;

namespace WealthTools.WebAPI.Proposals
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public class LowercaseContractResolver : DefaultContractResolver
        {
            protected override string ResolvePropertyName(string propertyName)
            {
                return propertyName.ToLower();
            }
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
           BaseApiStartupSetup.AddCommonServices(services);
            BaseApiStartupSetup.AddSwagger(services, "v1.0", "ProposalsAPI", "WealthTools.WebAPI.Proposals");
            BaseApiStartupSetup.AddAuthorization(services, Configuration);

            BaseApiStartupSetup.AddHealthCheck(services, Configuration,
                                new List<HealthCheck>() { HealthCheck.OracleConnection, HealthCheck.Redis });
            var sp = services.BuildServiceProvider();

            //AddProtectionService(services, Configuration, sp);
            BaseApiStartupSetup.AddProtectionService(services, Configuration, sp);
            
            // Add Project Specific Packages
            services.AddScoped<IProposalsRepository, ProposalsRepository>();
            services.AddScoped<IAccountsRepository, AccountsRepository>();
            services.AddScoped<IBrokerMgrRepository, BrokerMgrRepository>();
            services.AddScoped<IContactsRepository, ContactsRepository>();
            services.AddScoped<ISearchRepository, SearchRepository>();

            //Reports
            services.AddScoped<IReportManager, ReportManager>();
            services.AddScoped<IReportFactory, ProposalsReportFactory>();
            services.AddScoped<IReportsRepository, ReportsRepository>();

            #region Individual Report Classes
            services.AddTransient<CoverPage>();
            #endregion
            //Reports


            
        }

        //public static void AddProtectionService(IServiceCollection services, IConfiguration configuration, IServiceProvider serviceProvider)
        //{
        //    var configSection = configuration.GetSection("ProtectionServicesConfig");
        //    if (configSection != null)
        //    {
        //        if (configSection["Unprotect"] != "true")
        //        {
        //            //Add the helper so its availble for all classes.
        //            string Key = configSection["EncKey"];

        //            var ihelper = serviceProvider.GetRequiredService<IEncrypt>();

        //            EncryptHelper encryptHelper = new EncryptHelper(ihelper, Key);

        //            // services.AddScoped<IEncrtpyHelper, EncryptHelper>(s => new EncryptHelper(s.GetRequiredService<IEncrypt>(), Key));

        //            services.AddSingleton<IEncrtpyHelper, EncryptHelper>(s => new EncryptHelper(ihelper, Key));

        //            var sp = services.BuildServiceProvider();

        //            services.AddMvc().AddJsonOptions(opt =>
        //            {
        //                opt.SerializerSettings.ContractResolver = new ProtectedStringPropertyResolverX(sp) { };

        //            });
        //        }
        //    }
        //}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IHttpContextAccessor accessor, IWMLogger logger)
        {
            BaseApiStartupSetup.UseHealthChecks(app);
            BaseApiStartupSetup.UseExceptionHandler(app, env, logger, "ProposalsAPI");
            BaseApiStartupSetup.UseCors(app);
            BaseApiStartupSetup.UseSwagger(app, "../swagger/v1.0/swagger.json", "WealthTools.WebAPI.Proposals v1.0");
            app.UseAuthentication();
           // app.UseWealthToolsUsage();
            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
